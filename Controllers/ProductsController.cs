    using AutoMapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using MyApp.Controllers.Resources;
    using MyApp.Core.Models;
    using MyApp.Persistence;

    namespace MyApp.Controllers
    { 
        [Route("api/[controller]")]
        [ApiController]
        
        public class ProductsController : Controller
        {
            private readonly AppDbContext context;
            private readonly IMapper mapper;
            public ProductsController(AppDbContext context, IMapper mapper)
            {
                this.mapper = mapper;
                this.context = context;
                
            }
        // GET: api/Product-quantity
        [HttpGet("product-stock/{productid}")]
        public async Task<ActionResult<int>> GetProductStock(int productid)
        {
            var product = await context.Products.FindAsync(productid);
            if (product != null)
            {
                return Ok(product.Quantity);
            }
            return NotFound();
            
            }
            [HttpGet("products")]
            public async Task<IEnumerable<ProductResources>> GetProducts([FromQuery] int? categoryId = null,[FromQuery] string searchTerm = null){
                IQueryable<Product> query = context.Products.Include(p => p.Category);
                if(categoryId.HasValue){
                    query = query.Where(p => p.CategoryID == categoryId);
                }
                if(!string.IsNullOrWhiteSpace(searchTerm)){
                    query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
                }
                var products = await query.ToListAsync();
            var productresources =  mapper.Map<List<Product>,List<ProductResources>>(products);
                return productresources;
            }
            [HttpGet("category")]
            public async Task<IEnumerable<CategroyResources>> GetCategory(){
                var categories = await context.Categories.ToListAsync();
                var categoryresources = mapper.Map<List<Category>,List<CategroyResources>>(categories);
                return categoryresources;
            }
        
        }
    }