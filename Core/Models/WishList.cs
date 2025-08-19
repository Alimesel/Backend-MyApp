    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace MyApp.Core.Models
    {
        public class WishList
        {
         [Key]
         public int WishId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<WishlistItems> WishlistItems{ get; set; }
        }
    }