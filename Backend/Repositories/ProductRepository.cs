using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class ProductRepository : IProductRepository
{
    private readonly skillseekDbContext _dbContext;

    public ProductRepository(skillseekDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product> AddProductAsync(Product productDetails)
    {
        var result = _dbContext.Products.Add(productDetails);
        await _dbContext.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<int> DeleteProductAsync(int Id)
    {
        var filteredData = _dbContext.Products.Where(x => x.ID == Id).FirstOrDefault();
        _dbContext.Products.Remove(filteredData);
        return await _dbContext.SaveChangesAsync();
    }

    public async Task<Product> GetProductByIdAsync(int Id)
    {
        return await _dbContext.Products.Where(x => x.ID == Id).FirstOrDefaultAsync();
    }

    public async Task<List<Product>> GetProductListAsync()
    {
        return await _dbContext.Products.ToListAsync();
    }

    public async Task<int> UpdateProductAsync(Product productDetails)
    {
        _dbContext.Products.Update(productDetails);
        return await _dbContext.SaveChangesAsync();
    }
}
