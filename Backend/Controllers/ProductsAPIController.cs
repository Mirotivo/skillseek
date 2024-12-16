using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using skillseek.Models;
using MediatR;

namespace skillseek.Controllers;

[Route("api/products")]
[ApiController]
public class ProductsAPIController : BaseController
{
    private readonly IMediator mediator;
    private readonly IProductService _productService;

    public ProductsAPIController(IMediator mediator, IProductService productService)
    {
        this.mediator = mediator;
        _productService = productService;
    }

    [HttpGet]
    public IActionResult GetProducts(string query = "", int page = 1)
    {
        var products = _productService.GetProducts(query, page);
        return Ok(products);
    }

    // [HttpGet]
    // public async Task<List<Product>> GetProductListAsync()
    // {
    //     var productDetails = await mediator.Send(new GetProductListQuery());

    //     return productDetails;
    // }

    [HttpGet("productId")]
    public async Task<Product> GetProductByIdAsync(int productId)
    {
        var productDetails = await mediator.Send(new GetProductByIdQuery() { Id = productId });

        return productDetails;
    }

    [HttpPost]
    public async Task<Product> AddProductAsync(Product productDetails)
    {
        var productDetail = await mediator.Send(new CreateProductCommand(
            productDetails.Url,
        productDetails.IconClass,
        productDetails.Name,
        productDetails.ImageUrl,
        productDetails.CategoryID,
        productDetails.Title,
        productDetails.Description,
        productDetails.Price,
        productDetails.Category));
        return productDetail;
    }

    [HttpPut]
    public async Task<int> UpdateProductAsync(Product productDetails)
    {
        var isProductDetailUpdated = await mediator.Send(new UpdateProductCommand(
           productDetails.ID,
           productDetails.Url,
        productDetails.IconClass,
        productDetails.Name,
        productDetails.ImageUrl,
        productDetails.CategoryID,
        productDetails.Title,
        productDetails.Description,
        productDetails.Price,
        productDetails.Category));
        return isProductDetailUpdated;
    }

    [HttpDelete]
    public async Task<int> DeleteProductAsync(int Id)
    {
        return await mediator.Send(new DeleteProductCommand() { Id = Id });
    }

}

