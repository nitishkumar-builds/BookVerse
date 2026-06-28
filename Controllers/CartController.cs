using System.Security.Claims;
using AutoMapper;
using BookVerse.DataAccess.Repository.IRepository;
using BookVerse.Models;
using BookVerse.Models.ViewModels;
using BookVerse.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace BookVerse.Controllers
{
    [Authorize(Roles = SD.Role_Customer + "," + SD.Role_Company + "," + SD.Role_Admin + "," + SD.Role_Employee)]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CartController> _logger;

        public CartController(IUnitOfWork unitOfWork, ILogger<CartController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ── GET /Cart ─────────────────────────────────────────────────────────────
        public IActionResult Index()
        {
            var userId = GetUserId();

            var cartItems = _unitOfWork.ShoppingCart
                .GetAll(c => c.ApplicationUserId == userId, includeProperties: "Product")
                .ToList();

            foreach (var item in cartItems)
                item.Price = GetPriceByQuantity(item.Count, item.Product);

            var vm = new ShoppingCartVM
            {
                ShoppingCartList = cartItems,
                OrderHeader = new OrderHeader
                {
                    OrderTotal = cartItems.Sum(c => c.Price * c.Count)
                }
            };

            return View(vm);
        }

        // ── GET /Cart/Summary ─────────────────────────────────────────────────────
        public IActionResult Summary()
        {
            var userId = GetUserId();

            var cartItems = _unitOfWork.ShoppingCart
                .GetAll(c => c.ApplicationUserId == userId, includeProperties: "Product")
                .ToList();

            foreach (var item in cartItems)
                item.Price = GetPriceByQuantity(item.Count, item.Product);

            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            var vm = new ShoppingCartVM
            {
                ShoppingCartList = cartItems,
                OrderHeader = new OrderHeader
                {
                    ApplicationUserId = userId,
                    OrderTotal = cartItems.Sum(c => c.Price * c.Count),
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber ?? "",
                    StreetAddress = user.StreetAddress ?? "",
                    City = user.City ?? "",
                    State = user.State ?? "",
                    PostalCode = user.PostalCode ?? ""
                }
            };

            return View(vm);
        }

        // ── POST /Cart/Summary ────────────────────────────────────────────────────
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(ShoppingCartVM vm)
        {
            var userId = GetUserId();

            var cartItems = _unitOfWork.ShoppingCart
                .GetAll(c => c.ApplicationUserId == userId, includeProperties: "Product")
                .ToList();

            if (!cartItems.Any())
            {
                TempData["error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var item in cartItems)
                item.Price = GetPriceByQuantity(item.Count, item.Product);

            vm.ShoppingCartList = cartItems;
            vm.OrderHeader.OrderDate = DateTime.Now;
            vm.OrderHeader.ApplicationUserId = userId;
            vm.OrderHeader.OrderTotal = cartItems.Sum(c => c.Price * c.Count);
            vm.OrderHeader.Carrier ??= "";
            vm.OrderHeader.TrackingNumber ??= "";
            vm.OrderHeader.SessionId ??= "";
            vm.OrderHeader.PaymentIntentId ??= "";

            bool isCompanyUser = User.IsInRole(SD.Role_Company);

            if (isCompanyUser)
            {
                // Company accounts: delayed payment — order approved immediately
                vm.OrderHeader.OrderStatus = SD.StatusApproved;
                vm.OrderHeader.PaymentStatus = SD.PaymentStatusApprovedForDelayedPayment;
            }
            else
            {
                // Regular customers: pending until Stripe payment
                vm.OrderHeader.OrderStatus = SD.StatusPending;
                vm.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            }

            _unitOfWork.OrderHeader.Add(vm.OrderHeader);
            _unitOfWork.Save();

            // Create order detail lines
            foreach (var item in cartItems)
            {
                var detail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = vm.OrderHeader.Id,
                    Price = item.Price,
                    Count = item.Count
                };
                _unitOfWork.OrderDetail.Add(detail);
            }
            _unitOfWork.Save();

        if (!isCompanyUser)
            {
                if (!IsStripeConfigured())
                {
                    _unitOfWork.OrderHeader.UpdateStatus(
                        vm.OrderHeader.Id,
                        SD.StatusApproved,
                        SD.PaymentStatusApproved);
                    _unitOfWork.Save();

                    ClearCartForUser(userId);
                    TempData["success"] = "Order placed successfully. Stripe is not configured, so online payment was skipped.";
                    return RedirectToAction(nameof(OrderConfirmation), new { id = vm.OrderHeader.Id });
                }

                try
                {
                    var domain = $"{Request.Scheme}://{Request.Host.Value}/";
                    var options = new SessionCreateOptions
                    {
                        SuccessUrl = domain + $"Cart/OrderConfirmation?id={vm.OrderHeader.Id}",
                        CancelUrl = domain + "Cart/Index",
                        Mode = "payment",
                        LineItems = cartItems.Select(item => new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)Math.Round(item.Price * 100),
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.Product.Title
                                }
                            },
                            Quantity = item.Count
                        }).ToList()
                    };

                    var service = new SessionService();
                    Session session = service.Create(options);

                    _unitOfWork.OrderHeader.UpdateStripePaymentId(vm.OrderHeader.Id, session.Id, "");
                    _unitOfWork.Save();

                    return Redirect(session.Url);
                }
                catch (Stripe.StripeException ex)
                {
                    _logger.LogError(ex, "Stripe checkout session creation failed for order {OrderId}.", vm.OrderHeader.Id);
                    TempData["error"] = "Stripe checkout could not be started. Please try again.";
                    return RedirectToAction(nameof(Index));
                }
            }

            ClearCartForUser(userId);
            TempData["success"] = "Order placed successfully.";
            return RedirectToAction(nameof(OrderConfirmation), new { id = vm.OrderHeader.Id });
        }

        // ── GET /Cart/OrderConfirmation ───────────────────────────────────────────
        public IActionResult OrderConfirmation(int id)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == id);
            if (orderHeader == null)
            {
                return NotFound();
            }

            if (orderHeader.PaymentStatus != SD.PaymentStatusApprovedForDelayedPayment &&
                !string.IsNullOrWhiteSpace(orderHeader.SessionId))
            {
                try
                {
                    var service = new SessionService();
                    Session session = service.Get(orderHeader.SessionId);

                    if (session.PaymentStatus == "paid")
                    {
                        _unitOfWork.OrderHeader.UpdateStripePaymentId(
                            id,
                            orderHeader.SessionId,
                            session.PaymentIntentId ?? "");
                        _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);

                        ClearCartForUser(orderHeader.ApplicationUserId);
                        TempData["success"] = "Order placed successfully.";
                    }
                    else
                    {
                        TempData["error"] = "Payment was not completed. Please try checkout again.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Stripe.StripeException ex)
                {
                    _logger.LogError(ex, "Stripe payment verification failed for order {OrderId}.", id);
                    TempData["error"] = "We could not verify your Stripe payment. Please try again.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(id);
        }

        // ── POST /Cart/Plus ───────────────────────────────────────────────────────
        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);
            if (cart == null)
            {
                return NotFound();
            }

            if (cart.ApplicationUserId != GetUserId())
            {
                return Forbid();
            }

            cart.Count++;
            _unitOfWork.ShoppingCart.Update(cart);
            _unitOfWork.Save();
            TempData["success"] = "Cart updated.";
            return RedirectToAction(nameof(Index));
        }

        // ── POST /Cart/Minus ──────────────────────────────────────────────────────
        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);
            if (cart == null)
            {
                return NotFound();
            }

            if (cart.ApplicationUserId != GetUserId())
            {
                return Forbid();
            }

            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
            }
            else
            {
                cart.Count--;
                _unitOfWork.ShoppingCart.Update(cart);
            }

            _unitOfWork.Save();
            TempData["success"] = "Cart updated.";
            return RedirectToAction(nameof(Index));
        }

        // ── POST /Cart/Remove ─────────────────────────────────────────────────────
        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);
            if (cart == null)
            {
                return NotFound();
            }

            if (cart.ApplicationUserId != GetUserId())
            {
                return Forbid();
            }

            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            TempData["success"] = "Book removed from cart.";
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private void ClearCartForUser(string userId)
        {
            var cartItems = _unitOfWork.ShoppingCart
                .GetAll(c => c.ApplicationUserId == userId)
                .ToList();

            _unitOfWork.ShoppingCart.RemoveRange(cartItems);
            _unitOfWork.Save();
        }

        private static bool IsStripeConfigured()
        {
            var key = Stripe.StripeConfiguration.ApiKey;
            return !string.IsNullOrWhiteSpace(key) &&
                   key.StartsWith("sk_", StringComparison.OrdinalIgnoreCase) &&
                   !key.Contains("your-stripe", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the correct unit price based on quantity tier:
        ///   1-49   -> Product.Price
        ///   50-99  -> Product.Price50
        ///   100+   -> Product.Price100
        /// </summary>
        private static double GetPriceByQuantity(int quantity, Product product)
        {
            return quantity switch
            {
                < 50 => product.Price,
                < 100 => product.Price50,
                _ => product.Price100
            };
        }
    }
}
