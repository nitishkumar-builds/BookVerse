using BookVerse.DataAccess.Repository.IRepository;
using BookVerse.Models;
using BookVerse.Models.ViewModels;
using BookVerse.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookVerse.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<OrderHeader> orders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                // Admin and Employee see all orders
                orders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                // Customer and Company see only their own orders
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                orders = _unitOfWork.OrderHeader.GetAll(
                    filter: o => o.ApplicationUserId == userId,
                    includeProperties: "ApplicationUser");
            }

            return View(orders);
        }

        public IActionResult Details(int orderId)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(
                o => o.Id == orderId,
                includeProperties: "ApplicationUser");

            if (orderHeader == null)
            {
                return NotFound();
            }

            if (!User.IsInRole(SD.Role_Admin) && !User.IsInRole(SD.Role_Employee))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (orderHeader.ApplicationUserId != userId)
                {
                    return Forbid();
                }
            }

            var orderVM = new OrderVM
            {
                OrderHeader = orderHeader,
                OrderDetail = _unitOfWork.OrderDetail.GetAll(
                    d => d.OrderHeaderId == orderId,
                    includeProperties: "Product")
            };

            return View(orderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int orderId, string orderStatus)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == orderId);
            if (orderHeader == null)
            {
                return NotFound();
            }

            _unitOfWork.OrderHeader.UpdateStatus(orderId, orderStatus);
            _unitOfWork.Save();

            TempData["success"] = "Order status updated successfully.";
            return RedirectToAction(nameof(Details), new { orderId });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder(int orderId)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == orderId);
            if (orderHeader == null)
            {
                return NotFound();
            }

            _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.StatusShipped);
            _unitOfWork.Save();

            TempData["success"] = "Order marked as shipped.";
            return RedirectToAction(nameof(Details), new { orderId });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder(int orderId)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == orderId);
            if (orderHeader == null)
            {
                return NotFound();
            }

            _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.StatusCancelled);
            _unitOfWork.Save();

            TempData["success"] = "Order cancelled.";
            return RedirectToAction(nameof(Details), new { orderId });
        }
    }
}
