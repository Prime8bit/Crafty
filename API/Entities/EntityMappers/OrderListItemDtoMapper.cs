using CraftyCommon.DTOs;
using Mapster;

namespace API.Entities.EntityMappers
{
    public class OrderListItemDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Order, OrderListItemDto>()
                .Map(orderDto => orderDto.OrderDate, order => order.OrderDate.ToString("o"))
                .Map(orderDto => orderDto.BuyerName, order => order.Buyer == null ? null : order.Buyer.DisplayName)
                .GenerateMapper(MapType.Map | MapType.Projection | MapType.MapToTarget);
        }
    }
}
