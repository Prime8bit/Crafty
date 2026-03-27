using CraftyCommon.DTOs;
using Mapster;

namespace API.Entities.EntityMappers
{
    public class WishlistItemDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<WishlistItem, WishlistItemDto>()
                .GenerateMapper(MapType.Map | MapType.Projection | MapType.MapToTarget);
        }
    }
}
