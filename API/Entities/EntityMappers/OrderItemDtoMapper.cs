using CraftyCommon.DTOs;
using Mapster;

namespace API.Entities.EntityMappers
{
    public class OrderItemDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<OrderItem, OrderItemDto>()
                .Map(orderItemDto => orderItemDto.CraftName, orderItem => orderItem.Craft == null ? null : orderItem.Craft.Name)
                .Map(orderItemDto => orderItemDto.CraftMediaUrl, orderItem => 
                    orderItem.Craft == null || orderItem.Craft.SearchImage == null ? null : orderItem.Craft.SearchImage.Url)
                .Map(orderItemDto => orderItemDto.SellerDisplayName, orderItem => orderItem.Seller == null ? null : orderItem.Seller.DisplayName)
                .Map(orderItemDto => orderItemDto.SellerEmail, orderItem => orderItem.Seller == null ? null : orderItem.Seller.Email)
                .Map(orderItemDto => orderItemDto.SellerProfileImageUrl, orderItem => 
                    orderItem.Seller == null || orderItem.Seller.ProfileImage == null ? null : orderItem.Seller.ProfileImage.Url)
                .GenerateMapper(MapType.Map | MapType.Projection | MapType.MapToTarget);
        }
    }
}
