public interface ICategoryService
{
    List<Category> GetCategories();
    Category CreateCategory(Category category);
    Category UpdateCategory(int id, Category updatedCategory);
    bool DeleteCategory(int id);
}
