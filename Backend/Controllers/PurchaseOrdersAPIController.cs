using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using skillseek.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using ZXing;
using ZXing.QrCode;

namespace skillseek.Controllers;

[Route("api/purchaseorders")]
[ApiController]
public class PurchaseOrdersAPIController : BaseController
{
    private readonly IMapper _mapper;
    private readonly IPurchaseOrderService _purchaseOrderService;

    public PurchaseOrdersAPIController(
        IMapper mapper,
        IPurchaseOrderService purchaseOrderService)
    {
        _mapper = mapper;
        _purchaseOrderService = purchaseOrderService;
    }

    [HttpGet]
    public List<PurchaseOrderDto> GetPurchaseOrders()
    {
        var pos = _purchaseOrderService.GetPurchaseOrders();
        var posDto = _mapper.Map<List<PurchaseOrderDto>>(pos);
        // var posDto = pos.Select(po => ObjectMapper.MapToDto<PurchaseOrder, PurchaseOrderDto>(po)).ToList();
        return posDto;
    }

    [HttpPost]
    public IActionResult CreatePurchaseOrder([FromBody] PurchaseOrderDto purchaseOrderDto)
    {
        if (purchaseOrderDto == null)
        {
            return BadRequest("Invalid purchase order data");
        }

        var purchaseOrder = new PurchaseOrder
        {
            CustomerName = purchaseOrderDto.CustomerName,
            StationGroupID = purchaseOrderDto.StationGroupID,
            Status = purchaseOrderDto.Status,
            Timestamp = DateTime.UtcNow,
            Source = purchaseOrderDto.Source,
            OrderItems = purchaseOrderDto.OrderItems
                .Select(orderItemDto => new OrderItem
                {
                    Title = orderItemDto.Title,
                    Description = orderItemDto.Description,
                    Price = orderItemDto.Price,
                    Quantity = orderItemDto.Quantity,
                    CategoryID = orderItemDto.CategoryID
                })
                .ToList()
        };

        _purchaseOrderService.CreatePurchaseOrder(purchaseOrder);

        var createdPurchaseOrderDto = _mapper.Map<PurchaseOrderDto>(purchaseOrder);

        // Generate the QR code image for the URL
        var barcodeWriter = new ZXing.Windows.Compatibility.BarcodeWriter();
        barcodeWriter.Format = BarcodeFormat.QR_CODE;
        barcodeWriter.Options.Width = 200;
        barcodeWriter.Options.Height = 200;
        Bitmap qrCodeBitmap = barcodeWriter.Write($"https://localhost:8000/po={purchaseOrder.ID}");
        // Convert the QR code image to a base64-encoded string
        string base64Image;
        using (MemoryStream ms = new MemoryStream())
        {
            qrCodeBitmap.Save(ms, ImageFormat.Png);
            byte[] imageBytes = ms.ToArray();
            base64Image = Convert.ToBase64String(imageBytes);
        }
        createdPurchaseOrderDto.QRCode = base64Image;

        return Ok(createdPurchaseOrderDto);
    }
}

