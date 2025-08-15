    namespace MyApp.Controllers.Resources
    {
        public class ProductResources{
        
            public int ID { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public CategroyResources Category { get; set; }
        }
    }