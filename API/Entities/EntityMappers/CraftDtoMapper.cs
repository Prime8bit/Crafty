using CraftyCommon.DTOs;
using Mapster;

namespace API.Entities.EntityMappers
{
    public class CraftDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Craft, CraftDto>()
                .Map(craftDto => craftDto.CreatedAt, craft => craft.CreatedAt.ToString("o"))
                .Map(craftDto => craftDto.SellerDisplayName, craft => craft.Seller == null ? null : craft.Seller.DisplayName)
                .GenerateMapper(MapType.Map | MapType.Projection | MapType.MapToTarget);
        }
    }
}
