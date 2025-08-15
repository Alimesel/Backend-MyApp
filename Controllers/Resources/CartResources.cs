namespace MyApp.Controllers.Resources
{
    public class CartResources
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Size { get; set; }
    }
     public class RemoveResources
    {
        public int ProductId { get; set; }
        public string Size { get; set; }
    }
}