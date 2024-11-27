using MediatR;
public class CreateProductHandler : IRequestHandler<CreateProductCommand, Product>
{
    private readonly IProductRepository _productRepository;

    public CreateProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    public async Task<Product> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var productDetails = new Product()
        {
            Url = command.Url,
            Name = command.Name,
            ImageUrl = command.ImageUrl,
            CategoryID = command.CategoryID,
            Title = command.Title,
            Description = command.Description,
            Price = command.Price,
            Category = command.Category,
        };

        return await _productRepository.AddProductAsync(productDetails);
    }
}
