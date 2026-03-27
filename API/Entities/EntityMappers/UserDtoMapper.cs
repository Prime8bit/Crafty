using CraftyCommon.DTOs;
using Mapster;

namespace API.Entities.EntityMappers
{
    public class UserDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<User, UserDto>()
                .GenerateMapper(MapType.Map | MapType.Projection | MapType.MapToTarget);
        }
    }
}
