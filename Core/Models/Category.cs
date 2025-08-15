    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MyApp.Core.Models
    {
        [Table("Categories")]
        public class Category
        {
            [Key]
            public int CategoryID { get; set; }
            public string CategoryName { get; set; }
            public string CategoryImage { get; set; }
            [JsonIgnore]
            public ICollection<Product> Products { get; set; }
            public Category()
            {
                Products = new Collection<Product>();
            }
        }
    }