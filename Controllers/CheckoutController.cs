using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Controllers.Resources;
using MyApp.Core.Models;
using MyApp.Persistence;
using MyApp.StripePay;
using Stripe;
using Stripe.Checkout;
using StripeProduct = Stripe.Product; // ✅ Aliasing to avoid name conflict

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly StripeService _stripeService;
    private readonly IConfiguration _configuration;

    public CheckoutController(AppDbContext context, StripeService stripeService, IConfiguration configuration)
    {
        _context = context;
        _stripeService = stripeService;
        _configuration = configuration;
    }
     
     private int GetUserIdByToken() {
        var userid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userid);
    }
    [HttpPost("create-session")]
    public async Task<IActionResult> CreateSession([FromBody] CheckoutRequest request)
    {
        if (request == null || request.CartItems == null || !request.CartItems.Any())
            return BadRequest("Cart is empty.");

        var successUrl = "https://abbali.vercel.app/payment-success";
        var cancelUrl = "https://abbali.vercel.app/cart";

        var sessionId = await _stripeService.CreateCheckOutSession(
            request.CartItems.ToList(), request.UserId, successUrl, cancelUrl);

        return Ok(new { sessionId });
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        try
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _configuration["Stripe:WebhookSecret"],
                throwOnApiVersionMismatch: false
            );

            if (stripeEvent.Type == "checkout.session.completed")

            {
                var session = stripeEvent.Data.Object as Session;

                var service = new SessionService();
                var fullSession = await service.GetAsync(session.Id);

                await FulfillOrder(fullSession);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Webhook Error: {ex.Message}");
            return StatusCode(500);
        }
    }

    private async Task FulfillOrder(Session session)
    {
        var lineItemService = new SessionLineItemService();
        var lineItems = await lineItemService.ListAsync(session.Id, new SessionLineItemListOptions
        {
            Expand = new List<string> { "data.price.product" }
        });

        int userId = 0;
        if (session.Metadata != null && session.Metadata.ContainsKey("userId"))
            int.TryParse(session.Metadata["userId"], out userId);

        var order = new Orders
        {
            UserId = userId,
            TotalAmount = (decimal)((session.AmountTotal ?? 0) / 100.0),
            CreatedAt = DateTime.UtcNow,
            orderDetails = new List<OrderDetails>()
        };

        foreach (var item in lineItems)
        {
            int productId = 0;
            string productName = null;
            string productImageUrl = null;
            string size = null;

            if (item.Price?.Product is StripeProduct stripeProduct)
            {
                if (stripeProduct.Metadata != null && stripeProduct.Metadata.ContainsKey("productId"))
                {
                    int.TryParse(stripeProduct.Metadata["productId"], out productId);
                }

                productName = stripeProduct.Name;

                if (stripeProduct.Images != null && stripeProduct.Images.Count > 0)
                {
                    productImageUrl = stripeProduct.Images[0];
                }
                if (stripeProduct.Metadata.ContainsKey("size"))
                {
                    size = stripeProduct.Metadata["size"];
                }

            }
            

            order.orderDetails.Add(new OrderDetails
            {
                ProductId = productId,
                Name = productName,
                ImageUrl = productImageUrl,
                Quantity = (int)item.Quantity,
                Price = (decimal)(item.Price.UnitAmountDecimal / 100),
                Size = size
                
            });

            var dbProduct = await _context.Products.FindAsync(productId);
            if (dbProduct != null)
            {
                dbProduct.Quantity -= (int)item.Quantity;
            }
        }

        _context.Orders.Add(order);

        var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart != null)
        {
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.Carts.Remove(cart);
        }

        await _context.SaveChangesAsync();
    }

    [HttpGet("Get-Order-history")]
    public async Task<IActionResult> GetOrderHistory()
    {
        var userId = this.GetUserIdByToken();
        var order = await _context.Orders
        .Include(o => o.orderDetails)
        .Where(o => o.UserId == userId)
        .OrderByDescending(o => o.CreatedAt)
        .FirstOrDefaultAsync();
        if (order == null)
        {
            return NotFound("No Orders Yet");
        }
        return Ok(order);
    }
    

}
