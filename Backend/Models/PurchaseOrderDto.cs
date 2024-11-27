public class PurchaseOrderDto
{
    [MapsTo("ID")]
    public int ID { get; set; }
    [MapsTo("CustomerName")]
    public string CustomerName { get; set; }
    [MapsTo("StationGroupID")]
    public int StationGroupID { get; set; }
    [MapsTo("Status")]
    public string Status { get; set; }
    [MapsTo("Timestamp")]
    public DateTime Timestamp { get; set; }
    [MapsTo("Source")]
    public string Source { get; set; }
    [MapsTo("OrderItems")]
    public List<OrderItemDto> OrderItems { get; set; }
    [MapsTo("StationGroup")]
    public StationGroupDto StationGroup { get; set; }
    [MapsTo("Notes")]
    public string? Notes { get; set; }

    public string? QRCode { get; set; }
}
