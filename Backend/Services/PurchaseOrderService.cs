using Microsoft.EntityFrameworkCore;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly skillseekDbContext _db;

    public PurchaseOrderService(skillseekDbContext db)
    {
        _db = db;
    }

    public List<PurchaseOrder> GetPurchaseOrders()
    {
        return _db.PurchaseOrders
            .Include(po => po.OrderItems)
            .Include(po => po.StationGroup).ToList();
    }


    public PurchaseOrder CreatePurchaseOrder(PurchaseOrder purchaseOrder)
    {
        // Add the new purchase order to the database
        _db.PurchaseOrders.Add(purchaseOrder);
        try
        {
            _db.SaveChanges();
            return purchaseOrder;
        }
        catch (DbUpdateException ex)
        {
            throw; // Log it
        }
    }

    public async Task<PurchaseOrder> UpdatePurchaseOrder(PurchaseOrder purchaseOrder)
    {
        try
        {
            var existingPurchaseOrder = await _db.PurchaseOrders
                .Include(po => po.OrderItems)
                .FirstOrDefaultAsync(po => po.ID == purchaseOrder.ID);

            if (existingPurchaseOrder == null)
            {
                throw new ApplicationException("PurchaseOrder not found in the database.");
            }

            // Update properties of the existing PurchaseOrder entity
            existingPurchaseOrder.CustomerName = purchaseOrder.CustomerName;
            existingPurchaseOrder.Status = purchaseOrder.Status;
            existingPurchaseOrder.Timestamp = purchaseOrder.Timestamp;

            // Update related OrderItems ???

            await _db.SaveChangesAsync();
            return existingPurchaseOrder;
        }
        catch (Exception ex)
        {
            throw; // Log it
        }
    }
}
