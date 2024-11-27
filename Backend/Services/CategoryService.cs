
public class CategoryService : ICategoryService
{
    private readonly skillseekDbContext _db;

    public CategoryService(skillseekDbContext db)
    {
        _db = db;
    }

    public List<Category> GetCategories()
    {
        return _db.Categories.ToList();
    }

    public Category CreateCategory(Category category)
    {
        _db.Categories.Add(category);
        _db.SaveChanges();
        return category;
    }

    public Category UpdateCategory(int id, Category updatedCategory)
    {
        var category = _db.Categories.Find(id);
        if (category == null)
        {
            return null; // Or throw an exception
        }

        category.Name = updatedCategory.Name;
        _db.SaveChanges();
        return category;
    }

    public bool DeleteCategory(int id)
    {
        var category = _db.Categories.Find(id);
        if (category == null)
        {
            return false; // Or throw an exception
        }

        _db.Categories.Remove(category);
        _db.SaveChanges();
        return true;
    }
}
