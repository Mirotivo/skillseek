using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Orders;
using skillseek.Controllers;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/payments")]
public class PaymentsAPIController : BaseController
{
    private readonly PayPalOptions _payPalOptions;
    private readonly IPayPalPaymentService _paymentService;
    private readonly skillseekDbContext _dbContext;

    public PaymentsAPIController(IOptions<PayPalOptions> options, IPayPalPaymentService paymentService, skillseekDbContext dbContext)
    {
        _payPalOptions = options.Value;
        _paymentService = paymentService;
        _dbContext = dbContext;
    }

    [HttpPost("create-payment")]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDto request)
    {
        // Create the PayPal order
        var createResponse = await _paymentService.CreateOrder(
            request.Amount,
            request.Currency,
            _payPalOptions.ReturnUrl,
            _payPalOptions.ReturnUrl
        );

        if (createResponse.StatusCode != System.Net.HttpStatusCode.Created)
        {
            return BadRequest("Failed to create payment.");
        }

        var order = createResponse.Result<Order>();

        // Find the approval URL
        var approvalUrl = order.Links.FirstOrDefault(link => link.Rel == "approve")?.Href;
        if (string.IsNullOrEmpty(approvalUrl))
        {
            return BadRequest("Approval URL not found.");
        }

        // Redirect the user to the approval URL
        return Ok(new { orderID = order.Id, ApprovalUrl = approvalUrl });
    }

    [HttpPost("capture-payment")]
    public async Task<IActionResult> CapturePayment([FromBody] string orderId)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(orderId))
        {
            return BadRequest(new { success = false, message = "Order ID is required." });
        }

        var response = await _paymentService.CaptureOrder(orderId);
        if (response.StatusCode == System.Net.HttpStatusCode.Created)
        {
            // Capture payment was successful
            var subscription = new Subscription
            {
                UserId = userId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1), // Example for a 1-month subscription
                Status = "Active",
                Plan = "Student Pass",
                Amount = 69
            };

            // Save subscription to the database
            _dbContext.Subscriptions.Add(subscription);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Payment captured and subscription created successfully.",
                subscriptionId = subscription.Id
            });
        }

        return BadRequest(new
        {
            success = false,
            message = "Failed to capture payment.",
            orderId = orderId
        });
    }

    [HttpGet("success")]
    public async Task<IActionResult> PaymentSuccess(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { success = false, message = "Payment token is missing." });
        }

        // Capture the payment using the token (orderId)
        var response = await _paymentService.CaptureOrder(token);
        if (response.StatusCode == System.Net.HttpStatusCode.Created)
        {
            return Ok(new
            {
                success = true,
                message = "Payment captured successfully.",
                orderId = token
            });
        }

        return BadRequest(new
        {
            success = false,
            message = "Failed to capture payment.",
            orderId = token
        });
    }

    [HttpGet("cancel")]
    public IActionResult PaymentCancel()
    {
        return Ok(new
        {
            success = false,
            message = "Payment was canceled by the user."
        });
    }


    [HttpGet("history")]
    public IActionResult History()
    {
        var userId = GetUserId();

        // Fetch all relevant transactions involving the user
        var transactions = _dbContext.Transactions
            .Where(t => t.SenderId == userId || t.RecipientId == userId || t.RecipientId == null)
            .OrderBy(t => t.TransactionDate) // Order by date
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                SenderId = t.SenderId,
                SenderName = t.Sender.FirstName,
                RecipientId = t.RecipientId,
                RecipientName = t.Recipient != null ? t.Recipient.FirstName : "Platform",
                Amount = t.SenderId == userId ? -t.Amount : t.Amount,
                PlatformFee = t.PlatformFee,
                Net = t.SenderId == userId
                    ? -t.Amount - t.PlatformFee // Deduct platform fee for sender
                    : t.Amount - t.PlatformFee, // Deduct platform fee for recipient
                Status = t.Status,
                TransactionDate = t.TransactionDate,
                Date = t.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Description = t.RecipientId == null
                    ? $"Payment to Platform"
                    : t.SenderId == userId
                        ? $"Transferred to {t.Recipient.FirstName}"
                        : $"Received from {t.Sender.FirstName}",
                TransactionType = t.RecipientId == null
                    ? "To Platfothe rm"
                    : t.SenderId == userId
                        ? "Sent to User"
                        : "Received from User",
                Type = t.RecipientId == null
                    ? "transfer" // Transactions involving the platform are transfers
                    : "payment" // Transactions between users are payments
            })
            .ToList();

        // Fetch wallet balance
        var walletBalance = _dbContext.Wallets
            .Where(w => w.UserId == userId)
            .Select(w => w.Balance)
            .FirstOrDefault();

        // Response with chronological transaction history and balance
        return Ok(new PaymentHistoryDto
        {
            WalletBalance = walletBalance,
            TotalAmountCollected = 0,
            Transactions = transactions
        });
    }


    [HttpPost("add-paypal-account")]
    public IActionResult AddPayPalAccount([FromBody] AddPayPalAccountDto dto)
    {
        if (string.IsNullOrEmpty(dto.PayPalEmail))
        {
            return BadRequest("PayPal email is required.");
        }

        // Save the PayPal email to the user's account in the database
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = _dbContext.Users.Find(int.Parse(userId));
        if (user == null)
        {
            return NotFound();
        }

        user.PayPalAccountId = dto.PayPalEmail;
        _dbContext.SaveChanges();

        return Ok("PayPal account added successfully.");
    }

}
