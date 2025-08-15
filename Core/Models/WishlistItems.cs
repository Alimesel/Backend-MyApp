using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MyApp.Core.Models
{
    public class WishlistItems
    {
        [Key]
        public int WishlistItemId { get; set; }

        public int WishId { get; set; }  // Foreign key

        [ForeignKey(nameof(WishId))]
        [JsonIgnore]
        public WishList WishList { get; set; }  // Navigation property — required to avoid extra column

        public int ProductId { get; set; }

        public Product Product { get; set; }
    }


}