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
    private readonly IPayPalPaymentService _paymentService;
    private readonly skillseekDbContext _dbContext;

    public PaymentsAPIController(
        IOptions<StripeOptions> stripeOptions,
        IOptions<PayPalOptions> payPalOptions,
        IPayPalPaymentService paymentService,
        skillseekDbContext dbContext
    )
    {
        _stripeOptions = stripeOptions.Value;
        _payPalOptions = payPalOptions.Value;
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
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = SubscriptionStatus.Active,
                Plan = "Student Pass",
                Amount = 69
            };

            // Save subscription to the database
            _dbContext.Subscriptions.Add(subscription);

            // Create a transaction for the payment
            var transaction = new Transaction
            {
                SenderId = userId,
                RecipientId = null, // Payment to platform
                Amount = subscription.Amount,
                PlatformFee = 0, // Set to zero or a calculated fee
                TransactionDate = DateTime.UtcNow,
                Status = TransactionStatus.Completed,
                PaymentMethod = PaymentMethod.PayPal
            };

            _dbContext.Transactions.Add(transaction);

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


    [HttpPost("create-stripe-session")]
    public async Task<IActionResult> CreateStripeSession([FromBody] CreateStripeSessionDto request)
    {
        if (request.ListingId == null || request.Price <= 0)
        {
            return BadRequest(new { success = false, message = "Invalid tutor ID or price." });
        }

        try
        {
            // Initialize Stripe API key
            StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

            // Define the line items for the Checkout Session
            var lineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "aud", // Set currency
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Tutor Session - {request.ListingId}", // Dynamic product name
                        },
                        UnitAmount = (long)(request.Price * 100), // Amount in cents
                    },
                    Quantity = 1,
                }
            };

            // Create the Checkout Session options
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = $"http://localhost:4200/booking/{request.ListingId}",
                CancelUrl = "http://localhost:4200/payment-cancel",
            };

            // Create the Checkout Session
            var service = new SessionService();
            var session = await service.CreateAsync(options);

            // Return the session ID and URL
            return Ok(new { id = session.Id, url = session.Url });
        }
        catch (Exception ex)
        {
            // Handle errors
            return StatusCode(500, new { success = false, message = "Failed to create Stripe session.", error = ex.Message });
        }
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
            .Select(c => new
            {
                c.Id,
                c.Last4,
                c.ExpMonth,
                c.ExpYear,
                Type = "Card",
                Purpose = c.Type.ToString()
            })
            .ToList();

        return Ok(cards);
    }

}
