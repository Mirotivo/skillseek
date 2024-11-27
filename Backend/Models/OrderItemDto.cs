public class OrderItemDto
{
    [MapsTo("CategoryID")]
    public int CategoryID { get; set; }
    [MapsTo("Title")]
    public string Title { get; set; }
    [MapsTo("Description")]
    public string Description { get; set; }
    [MapsTo("Price")]
    public decimal Price { get; set; }
    [MapsTo("Quantity")]
    public int Quantity { get; set; }
}
