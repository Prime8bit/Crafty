using CraftyCommon.DTOs;
using Mapster;

namespace API.Entities.EntityMappers
{
    public class MessageConnectionDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MessageConnection, MessageConnectionDto>()
                .GenerateMapper(MapType.Map | MapType.Projection | MapType.MapToTarget);
        }
    }
}
