using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.Models;
using MyApp.Persistence;

namespace MyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishController : ControllerBase
    {
        private readonly AppDbContext _context;
        
        public WishController(AppDbContext context)
        {
            _context = context;
        }

        private int? GetUserIdByToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;

            return null;
        }

        [HttpGet("WishListProducts")]
        public async Task<IActionResult> GetWishListProducts()
        {
            var userId = GetUserIdByToken();
            if (userId == null)
                return Unauthorized("User ID claim not found.");

            var wishList = await _context.WishLists
                .Include(w => w.WishlistItems)
                .ThenInclude(wi => wi.Product)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(w => w.UserId == userId.Value);

            if (wishList == null)
                return Ok(new { Message = "Wishlist is empty", Items = new List<object>() });

            return Ok(wishList);
        }

        [HttpPost("AddProductToWishList")]
        public async Task<IActionResult> AddToWishList([FromBody] int productId)
        {
            var userId = GetUserIdByToken();
            if (userId == null)
                return Unauthorized("User ID claim not found.");

            // Check if product exists first
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound("Product not found.");

            var wishList = await _context.WishLists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == userId.Value);

            // Create wishlist if it doesn't exist
            if (wishList == null)
            {
                wishList = new WishList { UserId = userId.Value };
                await _context.WishLists.AddAsync(wishList);
                await _context.SaveChangesAsync(); // Save to get the WishId
            }

            // Check if product already in wishlist
            if (wishList.WishlistItems.Any(wi => wi.ProductId == productId))
                return BadRequest("Product already in wishlist.");

            // Add new wishlist item
            var wishlistItem = new WishlistItems 
            { 
                ProductId = productId, 
                WishId = wishList.WishId 
            };
            
            await _context.WishlistItems.AddAsync(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product added to wishlist." });
        }

        [HttpDelete("RemoveProductFromWishList/{productId}")]
        public async Task<IActionResult> RemoveFromWishList(int productId)
        {
            var userId = GetUserIdByToken();
            if (userId == null)
                return Unauthorized("User ID claim not found.");

            var wishList = await _context.WishLists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == userId.Value);

            if (wishList == null)
                return NotFound("Wishlist not found.");

            var wishlistItem = wishList.WishlistItems.FirstOrDefault(wi => wi.ProductId == productId);
            if (wishlistItem == null)
                return NotFound("Product not found in wishlist.");

            // Remove wishlist item
            _context.WishlistItems.Remove(wishlistItem);
            
            // Check if this was the last item and delete the wishlist if empty
            if (wishList.WishlistItems.Count == 1) // Current count includes the item we're about to remove
            {
                _context.WishLists.Remove(wishList);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product removed from wishlist." });
        }
    }
}