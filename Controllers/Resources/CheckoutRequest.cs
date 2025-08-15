namespace MyApp.Controllers.Resources
{
    public class CheckoutRequest
    {
        public int UserId { get; set; }
        public ICollection<CheckoutResources> CartItems { get; set; }
    }
    
}