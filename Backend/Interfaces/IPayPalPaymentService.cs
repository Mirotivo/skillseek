
public interface IPayPalPaymentService
{
    Task<PayPalHttp.HttpResponse> CreateOrder(decimal amount, string currency, string returnUrl, string cancelUrl);
    Task<PayPalHttp.HttpResponse> CaptureOrder(string orderId);
}
