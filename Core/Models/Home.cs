using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.Models
{
    public class Home
    {
        [Key]
        public int Id { get; set; }

        // e.g. "Clothes", "Shoes", "Accessories", or "MainBanner"
        [Required]
        public string Title { get; set; }

        // short description or subtitle
        public string Description { get; set; }

        // primary image (for cards or banners)
        public string? ImageUrl { get; set; }

        // optional: supporting image (for 3-image static sections)
        public string? ImageUrl2 { get; set; }
        public string? ImageUrl3 { get; set; }
        public string? ImageUrl4 { get; set; }

      
        public int DisplayOrder { get; set; }

        // show/hide flag
        public bool IsActive { get; set; } = true;
    }
}