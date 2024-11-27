using MediatR;
public class CreateProductCommand : IRequest<Product>
{
    public string Url { get; set; }
    public string IconClass { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public int CategoryID { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public double Price { get; set; }
    public Category Category { get; set; }

    public CreateProductCommand(string Url,
        string IconClass,
        string Name,
        string ImageUrl,
        int CategoryID,
        string Title,
        string Description,
        double Price,
        Category Category)
    {
        this.Url = Url;
        this.Name = Name;
        this.ImageUrl = ImageUrl;
        this.CategoryID = CategoryID;
        this.Title = Title;
        this.Description = Description;
        this.Price = Price;
        this.Category = Category;

    }
}
