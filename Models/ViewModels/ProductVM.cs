using System.ComponentModel.DataAnnotations;
using BookVerse.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookVerse.Models.ViewModels
{
    // Used for Index / Details / Delete / Home page product display
    public class ProductVM
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        [Display(Name = "List Price")]
        public double ListPrice { get; set; }

        public double Price { get; set; }

        public double Price50 { get; set; }

        public double Price100 { get; set; }

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public int CompanyId { get; set; }

        public string? CompanyName { get; set; }
    }

    // Used for Create / Edit product pages
    public class ProductUpsertVM
    {
        public Product Product { get; set; } = new();

        public IEnumerable<SelectListItem> CategoryList { get; set; } = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> CompanyList { get; set; } = Enumerable.Empty<SelectListItem>();

        // Holds newly uploaded image temporarily
        public IFormFile? File { get; set; }
    }
}