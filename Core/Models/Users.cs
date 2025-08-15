using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.Models
{
    public class User : IdentityUser<int>
    {
        [StringLength(50)]
        public string FirstName { get; set; }
        
        [StringLength(50)]
        public string LastName { get; set; }
        
        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; }
        
        public string Country { get; set; }
        
        public string City { get; set; }
    }
}
