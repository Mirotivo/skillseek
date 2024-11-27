using Microsoft.EntityFrameworkCore;

public interface IPurchaseOrderService
{
    List<PurchaseOrder> GetPurchaseOrders();

    PurchaseOrder CreatePurchaseOrder(PurchaseOrder purchaseOrder);
    Task<PurchaseOrder> UpdatePurchaseOrder(PurchaseOrder purchaseOrder);
}
