using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<CartItems> CartItems { get; set; } = new List<CartItems>();
    }
}