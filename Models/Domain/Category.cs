using System.ComponentModel.DataAnnotations;

namespace BookVerse.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Display Order must be between 1 and 100")]
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }
    }
}