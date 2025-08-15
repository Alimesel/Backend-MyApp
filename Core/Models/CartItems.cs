    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace MyApp.Core.Models
    {
        public class CartItems
        {
            [Key]
            public int CartItemId { get; set; }
        
            [ForeignKey("Cart")]
            public int CartId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public string  Size { get; set; }
            public Product Products { get; set; }
        }
    }