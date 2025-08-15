using MyApp.Controllers.Resources;
using Stripe;
using Stripe.Checkout;

namespace MyApp.StripePay
{
    public class StripeService
    {
        public StripeService(IConfiguration configuration)
        {
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        }

        public async Task<string> CreateCheckOutSession(List<CheckoutResources> items, int userId, string successUrl, string cancelUrl)
        {
            var lineItems = items.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmount = (long)(item.Price * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Name,
                        Metadata = new Dictionary<string, string>
                        {
                            { "productId", item.Productid.ToString() },
                             { "size",item.Size }
                        },
                        Images = new List<string>
                        {
                            item.ImageUrl
                        }
                    }
                },
                Quantity = item.Quantity,
                
            }).ToList();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return session.Id;
        }
    }
}
