using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Controllers.Resources;
using MyApp.Core.Models;
using MyApp.Persistence;

namespace MyApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        public CartController(AppDbContext context)
        {
            this._context = context;
        }
        private int GetUserIdByToken()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
        // Get Cart For Logged-in Users
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userid = this.GetUserIdByToken();
            var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Products)
            .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.UserId == userid);

            if (cart == null)
            {
                return Ok(new { CartItems = new List<Object>() });//empty Cart
            }
            return Ok(cart);
        }
        // Add Product To Cart
  [HttpPost("AddToCart")]
public async Task<IActionResult> AddToCart([FromBody] CartResources cartResources)
{
    try
    {
        var userid = this.GetUserIdByToken();

        // Start a transaction
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userid);

            if (cart == null)
            {
                cart = new Cart 
                { 
                    UserId = userid, 
                    CartItems = new List<CartItems>(),
                    CreatedAt = DateTime.UtcNow // Use UTC for consistency
                };
                _context.Carts.Add(cart); // Explicitly add new cart
                await _context.SaveChangesAsync(); // Save to generate CartId
            }

            var product = await _context.Products.FindAsync(cartResources.ProductId);
            if (product == null)
            {
                await transaction.RollbackAsync();
                return NotFound("Product not found");
            }

            // Rest of your existing logic for cart items...
            var existingCartItemsForProduct = cart.CartItems
                .Where(ci => ci.ProductId == cartResources.ProductId)
                .ToList();

            int totalQuantityForProductInCart = existingCartItemsForProduct.Sum(ci => ci.Quantity);

            if (totalQuantityForProductInCart + 1 > product.Quantity)
            {
                await transaction.RollbackAsync();
                return BadRequest("Product quantity is not enough");
            }

            var cartitem = existingCartItemsForProduct.FirstOrDefault(ci => ci.Size == cartResources.Size);

            if (cartitem == null)
            {
                cartitem = new CartItems
                {
                    ProductId = cartResources.ProductId,
                    Quantity = 1,
                    Size = cartResources.Size,
                    CartId = cart.CartId // Ensure this is set
                };
                cart.CartItems.Add(cartitem);
            }
            else
            {
                cartitem.Quantity++;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(cart);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // Log the error
            Console.WriteLine($"Error in AddToCart: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
    catch (Exception ex)
    {
        // Log the error
        Console.WriteLine($"Error in AddToCart (outer): {ex.Message}\n{ex.StackTrace}");
        return StatusCode(500, "An error occurred while processing your request");
    }
}
        [HttpPatch("addproductquantity/{productid}")]
        public async Task<IActionResult> AddQuantity([FromBody] RemoveResources removeResources)
        {
            var userid = this.GetUserIdByToken();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(c => c.Products)
                .FirstOrDefaultAsync(c => c.UserId == userid);

            if (cart == null)
            {
                return NotFound("Cart Not Found");
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci =>
                ci.ProductId == removeResources.ProductId &&
                ci.Size == removeResources.Size);

            if (cartItem == null)
            {
                return NotFound("Product Not Found in Cart");
            }

            // Get the product from the database
            var product = await _context.Products.FindAsync(removeResources.ProductId);
            if (product == null)
            {
                return NotFound("Product Not Found");
            }

            // ✅ Calculate total quantity of the same product (across all sizes) in the cart
            var totalQuantityForProduct = cart.CartItems
                .Where(ci => ci.ProductId == removeResources.ProductId)
                .Sum(ci => ci.Quantity);

            if (totalQuantityForProduct + 1 > product.Quantity)
            {
                return BadRequest("Cannot add more. Product stock limit reached (across all sizes).");
            }

            // ✅ Safe to add
            cartItem.Quantity++;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}