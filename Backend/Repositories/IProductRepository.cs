public interface IProductRepository
{
    public Task<List<Product>> GetProductListAsync();
    public Task<Product> GetProductByIdAsync(int Id);
    public Task<Product> AddProductAsync(Product productDetails);
    public Task<int> UpdateProductAsync(Product productDetails);
    public Task<int> DeleteProductAsync(int Id);
}
