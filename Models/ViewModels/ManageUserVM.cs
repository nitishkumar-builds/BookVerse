using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookVerse.Models.ViewModels
{
    public class ManageUserVM
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public string? CompanyName { get; set; }
        public string? City { get; set; }                  // ← add this
        public DateTimeOffset? LockoutEnd { get; set; }   // ← add this

        public IEnumerable<SelectListItem>? RoleList { get; set; }
    }
}