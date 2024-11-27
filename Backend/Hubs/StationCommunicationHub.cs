using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

public class PurchaseOrderListeners
{
    public PurchaseOrder PurchaseOrder { get; set; }
    public List<string> Listeners { get; set; }
}

public class StationCommunicationHub : Hub
{
    private readonly IPurchaseOrderService _purchaseOrderService;
    private static Dictionary<int, PurchaseOrderListeners> poListeners = new Dictionary<int, PurchaseOrderListeners>();
    private static ConcurrentDictionary<string, HashSet<string>> groupMemberships = new ConcurrentDictionary<string, HashSet<string>>();

    public StationCommunicationHub(IPurchaseOrderService purchaseOrderService)
    {
        _purchaseOrderService = purchaseOrderService;
    }
    public async Task SubscribeStation(string stationGroup)
    {
        // Remove from all groups this connection currently belongs to
        var connectionId = Context.ConnectionId;
        foreach (var group in groupMemberships.Keys)
        {
            if (groupMemberships[group].Contains(connectionId))
            {
                await Groups.RemoveFromGroupAsync(connectionId, group);
                groupMemberships[group].Remove(connectionId);
            }
        }

        // Add to the new group
        await Groups.AddToGroupAsync(connectionId, stationGroup);
        groupMemberships.AddOrUpdate(stationGroup, new HashSet<string>() { connectionId }, (key, oldValue) =>
        {
            oldValue.Add(connectionId);
            return oldValue;
        });
    }
    public async Task UnsubscribeStation(string stationGroup)
    {
        var connectionId = Context.ConnectionId;
        await Groups.RemoveFromGroupAsync(connectionId, stationGroup);
        if (groupMemberships.ContainsKey(stationGroup))
        {
            groupMemberships[stationGroup].Remove(connectionId);
        }
    }
    public async Task NotifyStations(PurchaseOrder po)
    {
        if (po.StationGroup != null && !string.IsNullOrEmpty(po.StationGroup.Name))
        {
            if (poListeners.ContainsKey(po.ID))
            {
                poListeners[po.ID].PurchaseOrder = po;
            }
            else
            {
                poListeners[po.ID] = new PurchaseOrderListeners { PurchaseOrder = po, Listeners = new List<string>() };
            }
            // Send the PO to all stations in the specified section
            await Clients.Group(po.StationGroup.Name).SendAsync("UpdateStation", po);
        }
        else
        {
            await Clients.Others.SendAsync("UpdateStation", po);
        }

        // Update the PurchaseOrder in the database
        await _purchaseOrderService.UpdatePurchaseOrder(po);
    }


    public async Task SubscribeClient(int poID)
    {
        var connectionId = Context.ConnectionId;

        // Remove the connection ID from all existing PurchaseOrderListeners
        foreach (var kvp in poListeners)
        {
            PurchaseOrderListeners poListener = kvp.Value;
            if (poListener.Listeners.Contains(connectionId))
            {
                poListener.Listeners.Remove(connectionId);
            }
        }

        // Register the connection ID for this client and the PO ID they want to track
        if (poListeners.ContainsKey(poID))
        {
            PurchaseOrderListeners poListener = poListeners[poID];
            poListener.Listeners.Add(connectionId);

            await Clients.Client(connectionId).SendAsync("UpdateClient", poListener.PurchaseOrder);
        }
    }
    public async Task UnsubscribeClient(int poID)
    {
        // Unregister the connection ID for this client and the PO ID they no longer want to track
        var connectionId = Context.ConnectionId;
        if (poListeners.ContainsKey(poID))
        {
            poListeners[poID].Listeners.Remove(connectionId);

            if (poListeners[poID].Listeners.Count == 0)
            {
                // Remove the PO from the dictionary if no clients are interested
                poListeners.Remove(poID);
            }
        }
    }
    public async Task NotifyClients(PurchaseOrder po)
    {
        await NotifyStations(po);
        // var updatedPo = await _purchaseOrderService.UpdatePurchaseOrder(po);
        if (poListeners.ContainsKey(po.ID))
        {
            poListeners[po.ID].PurchaseOrder = po;
            var listeners = poListeners[po.ID].Listeners;
            // Notify clients interested in this PO
            foreach (var connectionId in listeners)
            {
                await Clients.Client(connectionId).SendAsync("UpdateClient", po);
            }
        }
    }
}
