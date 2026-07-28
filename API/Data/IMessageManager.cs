using API.Entities;
using API.Misc;
using CraftyCommon.Pagination;
using CraftyCommon.DTOs;

namespace API.Data;

public interface IMessageManager
{
    Task<ManagerResponse<MessageDto>> AddMessage(long userId, CreateMessageDto messageDto);
    Task<ManagerResponse> DeleteMessage(long userId, long messageId);
    Task<ManagerResponse<MessageDto>> GetMessage(long userId, long messageId);
    Task<ManagerResponse<PagedList<MessageDto>>> GetMessageThread(long senderId, long recipientId, PaginationParams paginationParams);
    Task<IEnumerable<ContactDto>> GetContactsAsync(long userId);
    Task<ManagerResponse<MessageGroupDto>> AddConnectionToMessageGroup (string groupName, MessageConnection connection);
    Task<ManagerResponse<MessageGroupDto?>> RemoveMessageConnection (string connectionId);
    Task<MessageGroupDto?> GetMessageGroup(string groupName);
    Task<MessageGroupDto?> GetMessageGroupForConnection(string connectionId);
}