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
                return Ok(new { message = "Wishlist is empty.", items = new List<object>() });

            return Ok(wishList);
        }

        [HttpPost("AddProductToWishList")]
        public async Task<IActionResult> AddToWishList([FromBody] int productId)
        {
            var userId = GetUserIdByToken();
            if (userId == null)
                return Unauthorized("User ID claim not found.");

            var wishList = await _context.WishLists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == userId.Value);

            // If no wishlist, create one
            if (wishList == null)
            {
                wishList = new WishList
                {
                    UserId = userId.Value,
                    WishlistItems = new List<WishlistItems>()
                };
                await _context.WishLists.AddAsync(wishList);
                await _context.SaveChangesAsync(); // ensures WishId is generated
            }

            // Validate product
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound("Product not found.");

            // Check if already exists
            if (wishList.WishlistItems.Any(wi => wi.ProductId == productId))
                return BadRequest("Product already exists in wishlist.");

            // ✅ Use navigation property instead of WishId
            var wishlistItem = new WishlistItems
            {
                ProductId = productId,
                WishList = wishList
            };

            await _context.WishlistItems.AddAsync(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product added to wishlist." });
        }

        [HttpDelete("RemoveProductFromWishList")]
        public async Task<IActionResult> RemoveFromWishList([FromBody] int productId)
        {
            var userId = GetUserIdByToken();
            if (userId == null)
                return Unauthorized("User ID claim not found.");

            var wishList = await _context.WishLists
                .FirstOrDefaultAsync(w => w.UserId == userId.Value);

            if (wishList == null)
                return NotFound("Wishlist not found.");

            var wishlistItem = await _context.WishlistItems
                .FirstOrDefaultAsync(wi => wi.ProductId == productId && wi.WishId == wishList.WishId);

            if (wishlistItem == null)
                return NotFound("Product not found in wishlist.");

            _context.WishlistItems.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            // Check DB if wishlist is empty
            var remainingItems = await _context.WishlistItems
                .AnyAsync(wi => wi.WishId == wishList.WishId);

            if (!remainingItems)
            {
                _context.WishLists.Remove(wishList);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Product removed from wishlist." });
        }
    }
}
