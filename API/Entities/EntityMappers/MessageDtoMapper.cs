using CraftyCommon.DTOs;
using Mapster;

namespace API.Entities.EntityMappers
{
    public class MessageDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Message, MessageDto>()
                .Map(messageDto => messageDto.SenderDisplayName, message => message.Sender == null ? null : message.Sender.DisplayName)
                .Map(messageDto => messageDto.SenderProfileImageUrl, message => 
                    message.Sender == null || message.Sender.ProfileImage == null ? null : message.Sender.ProfileImage.Url)
                .Map(messageDto => messageDto.RecipientDisplayName, message => message.Recipient == null ? null : message.Recipient.DisplayName)
                .GenerateMapper(MapType.Map | MapType.Projection | MapType.MapToTarget);
        }
    }
}
