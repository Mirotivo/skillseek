public interface IStationGroupService
{
    List<StationGroup> GetStationGroups();
    StationGroup CreateStationGroup(StationGroup category);
    StationGroup UpdateStationGroup(int id, StationGroup updatedStationGroup);
    bool DeleteStationGroup(int id);
}
