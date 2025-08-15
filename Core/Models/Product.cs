using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MyApp.Core.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Quantity { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        [ForeignKey("Category")]
        [JsonIgnore]
        public int CategoryID { get; set; }
        
        public Category Category { get; set; }
    }
}