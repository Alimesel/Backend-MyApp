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
       // Add Product To Cart
[HttpPost("AddToCart")]
public async Task<IActionResult> AddToCart([FromBody] CartResources cartResources)
{
    var userid = this.GetUserIdByToken();

    var cart = await _context.Carts
        .Include(c => c.CartItems)
        .ThenInclude(ci => ci.Products)
        .ThenInclude(p => p.Category)
        .FirstOrDefaultAsync(c => c.UserId == userid);

    if (cart == null)
    {
        cart = new Cart { UserId = userid, CartItems = new List<CartItems>() };
    }

    var product = await _context.Products.FindAsync(cartResources.ProductId);
    if (product == null)
    {
        return NotFound();
    }

    // Get all cart items of this product regardless of size
    var existingCartItemsForProduct = cart.CartItems
        .Where(ci => ci.ProductId == cartResources.ProductId)
        .ToList();

    // Calculate total quantity for this product in the cart (all sizes)
    int totalQuantityForProductInCart = existingCartItemsForProduct.Sum(ci => ci.Quantity);

    // Check if adding one more exceeds product stock
    if (totalQuantityForProductInCart + 1 > product.Quantity)
    {
        return NotFound("Product quantity is not enough");
    }

    // Find cart item for requested size
    var cartitem = existingCartItemsForProduct.FirstOrDefault(ci => ci.Size == cartResources.Size);

    if (cartitem == null)
    {
        // Add new cart item for this size
        cartitem = new CartItems
        {
            ProductId = cartResources.ProductId,
            Quantity = 1,
            Size = cartResources.Size
        };
        cart.CartItems.Add(cartitem);
    }
    else
    {
        // Increase quantity for existing size
        cartitem.Quantity++;
    }

    _context.Carts.Update(cart);
    await _context.SaveChangesAsync();

    return Ok(cart);
}

        // Remove Product From Cart
        [HttpPost("remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveResources removeResources)
        {
            var userid = GetUserIdByToken();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userid);

            if (cart == null)
                return NotFound("Cart Not Found");

            var cartitem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == removeResources.ProductId
            && ci.Size == removeResources.Size);
            if (cartitem == null)
                return NotFound("Product Not Found");
            cart.CartItems.Remove(cartitem);
            if (!cart.CartItems.Any())
            {
                _context.Carts.Remove(cart);
            }
            await _context.SaveChangesAsync();
            return Ok(cart);
        }
        // Decrease product quantity
        [HttpPatch("productquantity/{productid}")]
        public async Task<IActionResult> DecreaseQuantity([FromBody] RemoveResources removeResources)
        {
            var userid = GetUserIdByToken();
            var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(c => c.Products)
            .FirstOrDefaultAsync(c => c.UserId == userid);
            if (cart == null)
            {
                return NotFound("Cart Not Found");
            }
            var cartItems = cart.CartItems.FirstOrDefault(ci => ci.ProductId == removeResources.ProductId && ci.Size == removeResources.Size);
            if (cartItems == null)
            {
                return NotFound("Product Not Found");
            }
            if (cartItems.Quantity > 1)
            {
                cartItems.Quantity = cartItems.Quantity - 1;
            }
            else
            {
                cart.CartItems.Remove(cartItems);
            }
            if (!cart.CartItems.Any())
            {
                _context.Carts.Remove(cart);
            }
            await _context.SaveChangesAsync();
            return Ok();
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