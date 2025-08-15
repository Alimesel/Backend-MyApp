using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Core.Models{
    [Table("Orders")]
    public class Orders{
        [Key]
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public ICollection<OrderDetails> orderDetails {get;set;}

       
    }
}