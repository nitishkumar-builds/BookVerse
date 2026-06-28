using AutoMapper;
using BookVerse.DataAccess.Data;
using BookVerse.DataAccess.Repository.IRepository;
using BookVerse.Models;
using BookVerse.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _db;

        public HomeController(IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _db = db;
        }

        public IActionResult Index()
        {
            // BUG FIX: Was returning View() with no model â€” homepage always showed empty catalog.
            // Now fetches all products with Category and Company included, then maps to ProductVM.
            var products = _unitOfWork.Product.GetAll(includeProperties: "Category,Company");

            // TEMP DEBUG â€” remove after image fix is confirmed
            foreach (var p in products)
                Console.WriteLine($"PRODUCT: {p.Title} | ImageUrl: [{p.ImageUrl}]");

            var productVMs = _mapper.Map<IEnumerable<ProductVM>>(products);
            return View(productVMs);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubscribeNewsletter(string email)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
            {
                TempData["error"] = "Please enter a valid email address.";
                return RedirectToAction(nameof(Index), null, null, "newsletter");
            }

            var subscriber = await _db.NewsletterSubscribers
                .FirstOrDefaultAsync(x => x.Email == email);

            if (subscriber == null)
            {
                _db.NewsletterSubscribers.Add(new NewsletterSubscriber
                {
                    Email = email,
                    SubscribedAt = DateTime.UtcNow,
                    IsActive = true
                });
                TempData["success"] = "Thanks for subscribing to BookVerse updates.";
            }
            else
            {
                subscriber.IsActive = true;
                TempData["success"] = "You are already subscribed to BookVerse updates.";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), null, null, "newsletter");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
