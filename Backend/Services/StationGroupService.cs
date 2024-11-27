
public class StationGroupService : IStationGroupService
{
    private readonly skillseekDbContext _db;

    public StationGroupService(skillseekDbContext db)
    {
        _db = db;
    }

    public List<StationGroup> GetStationGroups()
    {
        return _db.StationGroups.ToList();
    }

    public StationGroup CreateStationGroup(StationGroup stationGroup)
    {
        _db.StationGroups.Add(stationGroup);
        _db.SaveChanges();
        return stationGroup;
    }

    public StationGroup UpdateStationGroup(int id, StationGroup updatedStationGroup)
    {
        var stationGroup = _db.StationGroups.Find(id);
        if (stationGroup == null)
        {
            return null; // Or throw an exception
        }

        stationGroup.Name = updatedStationGroup.Name;
        _db.SaveChanges();
        return stationGroup;
    }

    public bool DeleteStationGroup(int id)
    {
        var stationGroup = _db.StationGroups.Find(id);
        if (stationGroup == null)
        {
            return false; // Or throw an exception
        }

        _db.StationGroups.Remove(stationGroup);
        _db.SaveChanges();
        return true;
    }
}
