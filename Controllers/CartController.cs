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
            _context = context;
        }

        private int GetUserIdByToken()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }

        // Get Cart For Logged-in Users
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserIdByToken();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Products)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return Ok(new { CartItems = new List<object>() }); // empty cart

            return Ok(cart);
        }

        // Add Product To Cart
       [HttpPost("AddToCart")]
public async Task<IActionResult> AddToCart([FromBody] CartResources cartResources)
{
    var userId = GetUserIdByToken();

    // Try to get the user's cart with items
    var cart = await _context.Carts
        .Include(c => c.CartItems)
        .ThenInclude(ci => ci.Products)
        .FirstOrDefaultAsync(c => c.UserId == userId);

    // If no cart exists, create a new one and save it immediately
    if (cart == null)
    {
        cart = new Cart
        {
            UserId = userId,
            CartItems = new List<CartItems>(),
            CreatedAt = DateTime.Now
        };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync(); // ✅ Save cart first to generate CartId
    }

    // Find the product
    var product = await _context.Products.FindAsync(cartResources.ProductId);
    if (product == null)
        return NotFound("Product not found");

    // Get all cart items of this product regardless of size
    var existingCartItemsForProduct = cart.CartItems
        .Where(ci => ci.ProductId == cartResources.ProductId)
        .ToList();

    // Check total quantity across all sizes
    int totalQuantityForProductInCart = existingCartItemsForProduct.Sum(ci => ci.Quantity);
    if (totalQuantityForProductInCart + 1 > product.Quantity)
        return BadRequest("Product stock is not enough");

    // Check if a cart item exists for this size
    var cartItem = existingCartItemsForProduct
        .FirstOrDefault(ci => ci.Size == cartResources.Size);

    if (cartItem == null)
    {
        // Add a new cart item for this size
        cartItem = new CartItems
        {
            CartId = cart.CartId, // ✅ assign CartId explicitly
            ProductId = cartResources.ProductId,
            Quantity = 1,
            Size = cartResources.Size
        };
        cart.CartItems.Add(cartItem);
    }
    else
    {
        // Increment quantity by 1
        cartItem.Quantity++;
    }

    await _context.SaveChangesAsync();

    return Ok(cart);
}

        // Remove Product From Cart
        [HttpPost("remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveResources removeResources)
        {
            var userId = GetUserIdByToken();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart Not Found");

            var cartItem = cart.CartItems.FirstOrDefault(ci =>
                ci.ProductId == removeResources.ProductId &&
                ci.Size == removeResources.Size);

            if (cartItem == null)
                return NotFound("Product Not Found");

            cart.CartItems.Remove(cartItem);

            // Remove cart if empty
            if (!cart.CartItems.Any())
                _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();
            return Ok(cart);
        }

        // Decrease product quantity
        [HttpPatch("productquantity/{productId}")]
        public async Task<IActionResult> DecreaseQuantity([FromBody] RemoveResources removeResources)
        {
            var userId = GetUserIdByToken();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Products)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart Not Found");

            var cartItem = cart.CartItems.FirstOrDefault(ci =>
                ci.ProductId == removeResources.ProductId &&
                ci.Size == removeResources.Size);

            if (cartItem == null)
                return NotFound("Product Not Found");

            if (cartItem.Quantity > 1)
                cartItem.Quantity--;
            else
                cart.CartItems.Remove(cartItem);

            if (!cart.CartItems.Any())
                _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();
            return Ok();
        }

        // Increase product quantity
        [HttpPatch("addproductquantity/{productId}")]
        public async Task<IActionResult> AddQuantity([FromBody] RemoveResources removeResources)
        {
            var userId = GetUserIdByToken();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Products)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart Not Found");

            var cartItem = cart.CartItems.FirstOrDefault(ci =>
                ci.ProductId == removeResources.ProductId &&
                ci.Size == removeResources.Size);

            if (cartItem == null)
                return NotFound("Product Not Found in Cart");

            var product = await _context.Products.FindAsync(removeResources.ProductId);
            if (product == null)
                return NotFound("Product Not Found");

            // Total quantity across all sizes
            var totalQuantity = cart.CartItems
                .Where(ci => ci.ProductId == removeResources.ProductId)
                .Sum(ci => ci.Quantity);

            if (totalQuantity + 1 > product.Quantity)
                return BadRequest("Cannot add more. Product stock limit reached.");

            cartItem.Quantity++;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
