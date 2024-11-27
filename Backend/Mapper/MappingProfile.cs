using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<PurchaseOrder, PurchaseOrderDto>();
        CreateMap<StationGroup, StationGroupDto>();

        CreateMap<OrderItemDto, OrderItem>();
        CreateMap<PurchaseOrderDto, PurchaseOrder>();
        CreateMap<StationGroupDto, StationGroup>();
    }
}
