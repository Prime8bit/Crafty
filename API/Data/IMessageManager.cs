using System.Text.RegularExpressions;
using API.DTOs;
using API.Entities;
using API.Misc;
using API.Pagination;

namespace API.Data;

public interface IMessageManager
{
    Task<ManagerResponse<MessageDto>> AddMessage(long userId, CreateMessageDto messageDto);
    Task<ManagerResponse> DeleteMessage(long userId, long messageId);
    Task<ManagerResponse<MessageDto>> GetMessage(long userId, long messageId);
    Task<ManagerResponse<PagedList<MessageDto>>> GetMessageThread(long senderId, long recipientId, PaginationParams paginationParams);
    Task<IEnumerable<ContactDto>> GetContactsAsync(long userId);
    Task<ManagerResponse<MessageGroupDto>> AddMessageGroup (MessageGroup group);
    Task<ManagerResponse<MessageGroupDto>> AddConnectionToMessageGroup (string groupName, MessageConnection connection);
    Task<bool> RemoveMessageConnection (string connectionId);
    Task<MessageConnectionDto?> GetMessageConnection(string connectionId);
    Task<MessageGroupDto?> GetMessageGroup(string groupName);
    Task<MessageGroupDto?> GetMessageGroupForConnection(string connectionId);
}