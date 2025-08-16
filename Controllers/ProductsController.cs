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
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Products/product-stock/{productid}
        [HttpGet("product-stock/{productid}")]
        public async Task<ActionResult<int>> GetProductStock(int productid)
        {
            var product = await _context.Products.FindAsync(productid);
            if (product != null)
                return Ok(product.Quantity);

            return NotFound();
        }

        // GET: api/Products/products
        [HttpGet("products")]
        public async Task<IEnumerable<ProductResources>> GetProducts(
            [FromQuery] int? categoryId = null,
            [FromQuery] string? searchTerm = null) // Nullable string
        {
            IQueryable<Product> query = _context.Products.Include(p => p.Category);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryID == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p =>
                    p.Name.Contains(searchTerm.Trim()) ||
                    p.Description.Contains(searchTerm.Trim()));

            var products = await query.ToListAsync();
            var productResources = _mapper.Map<List<Product>, List<ProductResources>>(products);
            return productResources;
        }

        // GET: api/Products/category
        [HttpGet("category")]
        public async Task<IEnumerable<CategroyResources>> GetCategory()
        {
            var categories = await _context.Categories.ToListAsync();
            var categoryResources = _mapper.Map<List<Category>, List<CategroyResources>>(categories);
            return categoryResources;
        }
    }
}
