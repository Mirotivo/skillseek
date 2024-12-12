using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Orders;
using skillseek.Controllers;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/payments")]
public class PaymentsAPIController : BaseController
{
    private readonly StripeOptions _stripeOptions;
    private readonly PayPalOptions _payPalOptions;
    private readonly skillseekDbContext _dbContext;
    private readonly PaymentGatewayFactory _paymentGatewayFactory;

    public PaymentsAPIController(
        IOptions<StripeOptions> stripeOptions,
        IOptions<PayPalOptions> payPalOptions,
        skillseekDbContext dbContext,
        PaymentGatewayFactory paymentGatewayFactory
    )
    {
        _stripeOptions = stripeOptions.Value;
        _payPalOptions = payPalOptions.Value;
        _dbContext = dbContext;
        _paymentGatewayFactory = paymentGatewayFactory;
    }

    [HttpPost("create-payment")]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDto request)
    {
        try
        {
            var gateway = _paymentGatewayFactory.GetPaymentGateway(request.Gateway);
            var paymentResult = await gateway.CreatePayment(request.Amount, request.Currency, request.ReturnUrl, request.CancelUrl);

            return Ok(new
            {
                success = true,
                paymentId = paymentResult.PaymentId,
                approvalUrl = paymentResult.ApprovalUrl
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("capture-payment")]
    public async Task<IActionResult> CapturePayment([FromBody] CapturePaymentRequestDto request)
    {
        try
        {
            var gatewayService = _paymentGatewayFactory.GetPaymentGateway(request.Gateway);
            var success = await gatewayService.CapturePayment(request.PaymentId);

            if (success)
            {
                // Assume a method to fetch the user ID from the current context
                var userId = GetUserId();

                // Create a subscription for the user
                var subscription = new Subscription
                {
                    UserId = userId,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    Status = SubscriptionStatus.Active,
                    Plan = "Student Pass", // Hardcoded; can be made dynamic
                    Amount = 69 // Hardcoded; can be parameterized
                };

                // Add the subscription to the database
                _dbContext.Subscriptions.Add(subscription);

                // Create a transaction record
                var transaction = new Transaction
                {
                    SenderId = userId,
                    RecipientId = null, // Payment to platform
                    Amount = subscription.Amount,
                    PlatformFee = 0, // Set to zero or a calculated fee
                    TransactionDate = DateTime.UtcNow,
                    PaymentType = PaymentType.StudentMembership,
                    PaymentMethod = request.Gateway == "PayPal" ? PaymentMethod.PayPal : PaymentMethod.Stripe,
                    Status = TransactionStatus.Completed
                };

                // Add the transaction to the database
                _dbContext.Transactions.Add(transaction);

                // Save changes to the database
                await _dbContext.SaveChangesAsync();

                // Return success response
                return Ok(new
                {
                    success = true,
                    message = "Payment captured and subscription created successfully.",
                    subscriptionId = subscription.Id
                });
            }

            return BadRequest(new { success = false, message = "Failed to capture payment." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("history")]
    public IActionResult History()
    {
        var userId = GetUserId();

        // Fetch all relevant transactions involving the user
        var transactions = _dbContext.Transactions
            .Where(t => t.SenderId == userId || t.RecipientId == userId)
            .OrderByDescending(t => t.TransactionDate) // Order by date
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                SenderId = t.SenderId,
                SenderName = t.Sender.FirstName,
                RecipientId = t.RecipientId,
                RecipientName = t.Recipient != null ? t.Recipient.FirstName : "Platform",
                Amount = t.SenderId == userId ? -(t.Amount + t.PlatformFee) : (t.Amount + t.PlatformFee),
                PlatformFee = t.PlatformFee,
                Net = t.SenderId == userId
                    ? -t.Amount
                    : t.Amount,
                Status = t.Status.ToString(),
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

        // Filter transactions with RecipientId == null as invoices
        var invoices = transactions.Where(t => t.RecipientId == null).ToList();
        // Exclude invoices from the transactions list
        transactions = transactions.Where(t => t.RecipientId != null).ToList();

        // Fetch wallet balance
        var walletBalance = _dbContext.Wallets
            .Where(w => w.UserId == userId)
            .Select(w => w.Balance)
            .FirstOrDefault();

        // Calculate the total amount collected by the user
        var totalAmountCollected = _dbContext.Transactions
            .Where(t => t.RecipientId == userId && t.Status == TransactionStatus.Completed)
            .AsEnumerable()
            .Sum(t => t.Amount);

        // Response with chronological transaction history and balance
        return Ok(new PaymentHistoryDto
        {
            WalletBalance = walletBalance,
            TotalAmountCollected = totalAmountCollected,
            Invoices = invoices,
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


    [HttpPost("save-card")]
    public async Task<IActionResult> SaveCard([FromBody] SaveCardDto request)
    {
        if (string.IsNullOrEmpty(request.StripeToken))
        {
            return BadRequest(new { success = false, message = "Card token is required." });
        }

        try
        {
            // Initialize Stripe API
            StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

            // Check if customer exists in your database
            var userId = GetUserId(); // Assume method to get logged-in user ID
            var user = _dbContext.Users.Find(userId);

            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                // Create a new Stripe customer
                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(new CustomerCreateOptions
                {
                    Email = user.Email, // Use user's email
                    Name = $"{user.FirstName} {user.LastName}",
                });

                // Save Stripe customer ID in the database
                user.StripeCustomerId = customer.Id;
                _dbContext.SaveChanges();
            }

            // Attach card to the Stripe customer
            var cardService = new CardService();
            var card = await cardService.CreateAsync(user.StripeCustomerId, new CardCreateOptions
            {
                Source = request.StripeToken, // Use the token from the frontend
            });

            // Save card details in the database
            var userCard = new UserCard
            {
                UserId = user.Id,
                CardId = card.Id,
                Last4 = card.Last4,
                ExpMonth = card.ExpMonth,
                ExpYear = card.ExpYear,
                Type = request.Purpose == "Receiving" ? CardType.Receiving : CardType.Paying,
            };

            _dbContext.UserCards.Add(userCard);
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Card saved successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to save card.", error = ex.Message });
        }
    }

    [HttpDelete("remove-card/{Id}")]
    public async Task<IActionResult> RemoveCard(int Id)
    {
        var userId = GetUserId();

        var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null || string.IsNullOrEmpty(user.StripeCustomerId))
        {
            return BadRequest(new { success = false, message = "User or Stripe customer not found." });
        }

        // Find the card in the database
        var card = _dbContext.UserCards.FirstOrDefault(c => c.Id == Id && c.UserId == userId);
        if (card == null)
        {
            return NotFound(new { success = false, message = "Card not found." });
        }

        try
        {
            // Remove the card from Stripe
            StripeConfiguration.ApiKey = _stripeOptions.ApiKey;
            var cardService = new CardService();
            await cardService.DeleteAsync(user.StripeCustomerId, card.CardId);

            // Remove the card from the database
            _dbContext.UserCards.Remove(card);
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Card removed successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to remove card.", error = ex.Message });
        }
    }

    [HttpGet("saved-cards")]
    public IActionResult GetSavedCards()
    {
        var userId = GetUserId();

        // Fetch saved cards for the user
        var cards = _dbContext.UserCards
            .Where(c => c.UserId == userId)
            .Select(c => new CardDto
            {
                Id = c.Id,
                Last4 = c.Last4,
                ExpMonth = c.ExpMonth,
                ExpYear = c.ExpYear,
                Type = "Card",
                Purpose = c.Type.ToString()
            })
            .ToList();

        return Ok(cards);
    }

}
