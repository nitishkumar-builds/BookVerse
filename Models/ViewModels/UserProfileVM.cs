using System.ComponentModel.DataAnnotations;

namespace BookVerse.Models.ViewModels
{
    public class UserProfileVM
    {
        [Required]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        [Display(Name = "Company")]
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? Role { get; set; }
    }
}