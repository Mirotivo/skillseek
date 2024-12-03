using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/wallets")]
[ApiController]
public class WalletsAPIController : BaseController
{

    private readonly skillseekDbContext _dbContext;
    private readonly IPayPalPaymentService _payPalPaymentService;

    public WalletsAPIController(skillseekDbContext dbContext, IPayPalPaymentService payPalPaymentService)
    {
        _dbContext = dbContext;
        _payPalPaymentService = payPalPaymentService;
    }

    [HttpPost("add-money")]
    public async Task<IActionResult> AddMoneyToWallet([FromBody] AddMoneyRequestDto request)
    {
        var userId = GetUserId();

        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than zero.");

        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
            return NotFound("Wallet not found.");

        // Call PayPal API to create payment
        var payPalResponse = await _payPalPaymentService.CreateOrder(request.Amount, "USD", "", "");
        if (payPalResponse.StatusCode != System.Net.HttpStatusCode.Created)
            return BadRequest("Payment creation failed.");

        // Update wallet balance
        wallet.Balance += request.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        // Log the transaction
        var transaction = new Transaction
        {
            SenderId = userId,
            RecipientId = null, // Platform
            Amount = request.Amount,
            PlatformFee = 0,
            TransactionDate = DateTime.UtcNow,
            Status = "Completed",
            PaymentMethod = request.PaymentMethod
        };
        _dbContext.Transactions.Add(transaction);

        await _dbContext.SaveChangesAsync();

        return Ok(new { WalletBalance = wallet.Balance, TransactionId = transaction.Id });
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetWalletBalance()
    {
        var userId = GetUserId();

        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
            return NotFound("Wallet not found.");

        return Ok(new { Balance = wallet.Balance, LastUpdated = wallet.UpdatedAt });
    }
}

