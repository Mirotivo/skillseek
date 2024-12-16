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
    private readonly IWalletService _walletService;
    private readonly PaymentGatewayFactory _paymentGatewayFactory;

    public WalletsAPIController(
        IWalletService walletService,
        PaymentGatewayFactory paymentGatewayFactory
    )
    {
        _walletService = walletService;
        _paymentGatewayFactory = paymentGatewayFactory;
    }

    [HttpPost("add-money")]
    public async Task<IActionResult> AddMoneyToWallet([FromBody] PaymentRequestDto request)
    {
        var userId = GetUserId();

        try
        {
            var result = await _walletService.AddMoneyToWallet(userId, request);
            return Ok(new { result.PaymentId, result.ApprovalUrl, result.TransactionId });
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

        try
        {
            await _walletService.CapturePayment(userId, request.PaymentId);
            return Ok(new { Message = "Payment captured successfully." });
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

        try
        {
            var result = await _walletService.GetWalletBalance(userId);
            return Ok(new { Balance = result.Balance, LastUpdated = result.LastUpdated });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "Failed to fetch wallet balance.", Error = ex.Message });
        }
    }
}

