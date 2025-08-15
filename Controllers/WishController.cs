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
                return NotFound("Wishlist not found.");

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

            if (wishList == null)
            {
                wishList = new WishList
                {
                    UserId = userId.Value,
                    WishlistItems = new List<WishlistItems>()
                };
                await _context.WishLists.AddAsync(wishList);
            }

            // Check if product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound("Product not found.");

            // Check if product already in wishlist
                if (wishList.WishlistItems.Any(wi => wi.ProductId == productId))
                    return BadRequest();

            // Add new wishlist item
            var wishlistItem = new WishlistItems { ProductId = productId, WishList = wishList };
            wishList.WishlistItems.Add(wishlistItem);

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
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == userId.Value);

            if (wishList == null)
                return NotFound("Wishlist not found.");

            var wishlistItem = wishList.WishlistItems.FirstOrDefault(wi => wi.ProductId == productId);
            if (wishlistItem == null)
                return NotFound("Product not found in wishlist.");

            // Remove wishlist item
            wishList.WishlistItems.Remove(wishlistItem);
            _context.WishlistItems.Remove(wishlistItem);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
