using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MyApp.Core.Models
{
    [Table("OrderDetails")]
    public class OrderDetails
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Orders")]
        public string Name { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
         public string ImageUrl { get; set; }
         public string Size { get; set; }
         [JsonIgnore]
        public Orders Order { get; set; }

       

    }
}
