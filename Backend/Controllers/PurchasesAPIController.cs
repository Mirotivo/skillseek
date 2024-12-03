using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/purchases")]
[ApiController]
public class PurchasesAPIController : BaseController
{

    private readonly skillseekDbContext _dbContext;

    public PurchasesAPIController(skillseekDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePurchase([FromBody] PurchaseRequestDto request)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == request.UserId);
        if (wallet == null)
            return NotFound("Wallet not found.");

        if (wallet.Balance < request.Amount)
            return BadRequest("Insufficient wallet balance.");

        // Deduct amount from wallet
        wallet.Balance -= request.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        // Log the transaction
        var transaction = new Transaction
        {
            SenderId = request.UserId,
            RecipientId = request.RecipientId,
            Amount = request.Amount,
            PlatformFee = request.Amount * 0.05m, // Example: 5% platform fee
            TransactionDate = DateTime.UtcNow,
            Status = "Completed",
            PaymentMethod = "Wallet"
        };
        _dbContext.Transactions.Add(transaction);

        await _dbContext.SaveChangesAsync();

        return Ok(new { WalletBalance = wallet.Balance, TransactionId = transaction.Id });
    }

    [HttpGet("")]
    public async Task<IActionResult> GetUserPurchases()
    {
        var userId = GetUserId();

        var purchases = await _dbContext.Transactions
            .Where(t => t.SenderId == userId && t.RecipientId != null)
            .ToListAsync();

        if (purchases == null || purchases.Count == 0)
            return NotFound("No purchases found.");

        return Ok(purchases);
    }

}

