using CraftyCommon.DTOs;
using Mapster;

namespace API.Entities.EntityMappers
{
    public class OrderDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Order, OrderDto>()
                .Map(orderDto => orderDto.OrderDate, order => order.OrderDate.ToString("o"))
                .Map(orderDto => orderDto.BuyerDisplayName, order => order.Buyer == null ? null : order.Buyer.DisplayName)
                .Map(orderDto => orderDto.BuyerEmail, order => order.Buyer == null ? null : order.Buyer.Email)
                .Map(orderDto => orderDto.BuyerProfileImageUrl, order => 
                    order.Buyer == null || order.Buyer.ProfileImage == null ? null : order.Buyer.ProfileImage.Url)
                .GenerateMapper(MapType.Map | MapType.Projection | MapType.MapToTarget);
        }
    }
}
