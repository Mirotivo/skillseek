using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;

public class PayPalPaymentService : IPayPalPaymentService
{
    private readonly skillseekDbContext _db;
    private readonly PayPalHttpClient _client;

    public PayPalPaymentService(skillseekDbContext db, IOptions<PayPalOptions> options)
    {
        _db = db;

        var payPalOptions = options.Value;
        PayPalEnvironment environment = payPalOptions.Environment.ToLower() switch
        {
            "sandbox" => new SandboxEnvironment(payPalOptions.ClientId, payPalOptions.ClientSecret),
            "live" => new LiveEnvironment(payPalOptions.ClientId, payPalOptions.ClientSecret),
            _ => throw new InvalidOperationException("Invalid PayPal environment configuration. Use 'Sandbox' or 'Live'.")
        };

        // Initialize PayPal client
        _client = new PayPalHttpClient(environment);
    }

public async Task<PayPalHttp.HttpResponse> CreateOrder(decimal amount, string currency, string returnUrl, string cancelUrl)
{
    var request = new OrdersCreateRequest();
    request.Prefer("return=representation");
    request.RequestBody(new OrderRequest
    {
        CheckoutPaymentIntent = "CAPTURE",
        ApplicationContext = new ApplicationContext
        {
            ReturnUrl = returnUrl,
            CancelUrl = cancelUrl
        },
        PurchaseUnits = new List<PurchaseUnitRequest>
        {
            new PurchaseUnitRequest
            {
                AmountWithBreakdown = new AmountWithBreakdown
                {
                    CurrencyCode = currency,
                    Value = amount.ToString("F2")
                }
            }
        }
    });

    try
    {
        var response = await _client.Execute(request);
        return response;
    }
    catch (HttpException ex)
    {
        Console.WriteLine($"Error: {ex.StatusCode} {ex.Message}");
        throw;
    }
}

    public async Task<PayPalHttp.HttpResponse> CaptureOrder(string orderId)
    {
        var request = new OrdersCaptureRequest(orderId);
        request.RequestBody(new OrderActionRequest());

        try
        {
            var response = await _client.Execute(request);
            return response;
        }
        catch (HttpException ex)
        {
            Console.WriteLine($"Error: {ex.StatusCode} {ex.Message}");
            throw;
        }
    }
}
