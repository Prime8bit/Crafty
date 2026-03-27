using CraftyCommon.DTOs;
using Mapster;

namespace API.Entities.EntityMappers
{
    public class MessageGroupDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MessageGroup, MessageGroupDto>()
                .GenerateMapper(MapType.Map | MapType.Projection | MapType.MapToTarget);
        }
    }
}
