using CraftyCommon.DTOs;
using Mapster;

namespace API.Entities.EntityMappers
{
    public class MediaDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Media, MediaDto>()
                .GenerateMapper(MapType.Map | MapType.Projection | MapType.MapToTarget);

            config.NewConfig<CraftMedia, CraftMediaDto>()
                .Map(craftMediaDto => craftMediaDto.CraftName, craftMedia => craftMedia.Craft == null ? null : craftMedia.Craft.Name)
                .GenerateMapper(MapType.Map | MapType.Projection | MapType.MapToTarget);

            config.NewConfig<UserMedia, UserMediaDto>()
                .Map(userMediaDto => userMediaDto.UserUserName, craftMedia => craftMedia.User == null ? null : craftMedia.User.UserName)
                .GenerateMapper(MapType.Map | MapType.Projection | MapType.MapToTarget);
        }
    }
}
