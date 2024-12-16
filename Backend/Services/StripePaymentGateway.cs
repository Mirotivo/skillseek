using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly StripeOptions _stripeOptions;

    public StripePaymentGateway(IOptions<StripeOptions> stripeOptions)
    {
        _stripeOptions = stripeOptions.Value;
    }

    public async Task<PaymentResult> CreatePayment(decimal amount, string currency, string returnUrl, string cancelUrl)
    {
        StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions { Name = "Order Payment" },
                        UnitAmount = (long)(amount * 100),
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = returnUrl,
            CancelUrl = cancelUrl,
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return new PaymentResult
        {
            PaymentId = session.Id,
            ApprovalUrl = session.Url
        };
    }

    public async Task<bool> CapturePayment(string paymentId)
    {
        // Stripe automatically captures payments when using Sessions
        return true;
    }

    public async Task<string> RefundPayment(string paymentId, decimal amount)
    {
        StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

        var refundService = new RefundService();
        var refund = await refundService.CreateAsync(new RefundCreateOptions
        {
            PaymentIntent = paymentId,
            Amount = (long)(amount * 100),
        });

        return refund.Id;
    }

    public async Task<bool> ProcessPayment(Transaction transaction)
    {
        try
        {
            StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

            var options = new ChargeCreateOptions
            {
                Amount = (long)((transaction.Amount + transaction.PlatformFee) * 100),
                Currency = "AUD",
                Customer = transaction.Sender.StripeCustomerId.ToString(),
                Description = $"Payment for Lesson ID {transaction.Id}",
            };

            var service = new ChargeService();
            var charge = service.Create(options);

            return charge.Status == "succeeded";
        }
        catch (Exception ex)
        {
            // Log error
            throw new ApplicationException("Stripe payment processing failed", ex);
        }
    }

}
