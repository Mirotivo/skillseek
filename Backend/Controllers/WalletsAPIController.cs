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
    private readonly PaymentGatewayFactory _paymentGatewayFactory;

    public WalletsAPIController(skillseekDbContext dbContext, PaymentGatewayFactory paymentGatewayFactory)
    {
        _dbContext = dbContext;
        _paymentGatewayFactory = paymentGatewayFactory;
    }

    [HttpPost("add-money")]
    public async Task<IActionResult> AddMoneyToWallet([FromBody] PaymentRequestDto request)
    {
        var userId = GetUserId();

        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than zero.");

        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
            return NotFound("Wallet not found.");

        // Use PayPalPaymentGateway via IPaymentGateway
        var paymentGateway = _paymentGatewayFactory.GetPaymentGateway("PayPal");

        try
        {
            // Create a payment
            var paymentResult = await paymentGateway.CreatePayment(request.Amount, "AUD", request.ReturnUrl, request.CancelUrl);

            if (string.IsNullOrEmpty(paymentResult.PaymentId) || string.IsNullOrEmpty(paymentResult.ApprovalUrl))
                return BadRequest("Failed to create payment.");

            // Log the transaction as Pending
            var transaction = new Transaction
            {
                SenderId = userId,
                RecipientId = null, // Platform
                Amount = request.Amount,
                PlatformFee = 0,
                TransactionDate = DateTime.UtcNow,
                PaymentType = PaymentType.WalletTopUp,
                PaymentMethod = PaymentMethod.PayPal,
                Status = TransactionStatus.Pending,
                PaymentId = paymentResult.PaymentId
            };
            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                PaymentId = paymentResult.PaymentId,
                ApprovalUrl = paymentResult.ApprovalUrl,
                TransactionId = transaction.Id
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "Failed to process payment.", Error = ex.Message });
        }
    }

    [HttpPost("capture-payment")]
    public async Task<IActionResult> CapturePaymentForWallet([FromBody] CapturePaymentRequestDto request)
    {
        var userId = GetUserId();

        // Use PayPalPaymentGateway via IPaymentGateway
        var paymentGateway = _paymentGatewayFactory.GetPaymentGateway("PayPal");

        try
        {
            // Capture the payment
            var success = await paymentGateway.CapturePayment(request.PaymentId);

            if (!success)
                return BadRequest("Failed to capture payment.");

            // Find the transaction
            var transaction = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.PaymentId == request.PaymentId && t.SenderId == userId);
            if (transaction == null)
                return NotFound("Transaction not found.");

            // Update the transaction status
            transaction.Status = TransactionStatus.Completed;

            // Update the wallet balance
            var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
                return NotFound("Wallet not found.");

            wallet.Balance += transaction.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new { WalletBalance = wallet.Balance, TransactionId = transaction.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "Failed to capture payment.", Error = ex.Message });
        }
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

