using API.Data.Configuration;
using API.DTOs;
using API.Entities;
using API.Misc;
using API.Pagination;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageManager(DataContext context, ICraftyUserManager userManager) : IMessageManager
{
    public async Task<ManagerResponse<MessageDto>> GetMessage(long userId, long messageId)
    {

        var result = await context.Messages
            .Include(message => message.Sender)
                .ThenInclude(user => user.ProfileImage)
            .Include(message => message.Recipient)
            .SingleOrDefaultAsync(message => message.Id == messageId);
        
        if (result == null)
        {
            return new ManagerResponse<MessageDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages =  [$"Message with id {messageId} does not exist."]
            };
        }

        if (result.SenderId != userId)
        {
            return new ManagerResponse<MessageDto>()
            {
                ResponseType = ManagerResponseType.Forbidden
            };
        }
            
        return new ManagerResponse<MessageDto>(new MessageDto(result));
    }

    public async Task<ManagerResponse<PagedList<MessageDto>>> GetMessageThread(long senderId, long recipientId, PaginationParams paginationParams)
    {        
        var sender = await userManager.GetUserAsync(senderId);
        
        if (sender == null)
        {
            return new ManagerResponse<PagedList<MessageDto>>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"User with id {senderId} does not exist."]
            };
        }

        var recipient = await userManager.GetUserAsync(recipientId);
        
        if (recipient == null)
        {
            return new ManagerResponse<PagedList<MessageDto>>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"User with id {recipientId} does not exist."]
            };
        }
        
        // First update all messages to be marked as read
        await context.Messages
            .Where(message => message.RecipientId == sender.Id 
                && message.SenderId == recipient.Id
                && message.DateRead == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(message => message.DateRead, DateTime.UtcNow));

        var query = context.Messages
            .Include(message => message.Sender)
                .ThenInclude(user => user.ProfileImage)
            .Include(message => message.Recipient)
            .Where(message => (message.RecipientId == sender.Id && message.SenderId == recipient.Id && !message.RecipientDeleted)
                || (message.SenderId == sender.Id && message.RecipientId == recipient.Id && !message.SenderDeleted))
            .OrderByDescending(message => message.DateSent)
            .Select(message => new MessageDto(message));

        var result = await PagedList<MessageDto>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);

        return new ManagerResponse<PagedList<MessageDto>>(result);
    }    

    public async Task<ManagerResponse<MessageDto>> AddMessage(long userId, CreateMessageDto messageDto)
    {        
        var sender = await userManager.GetUserAsync(userId);
        var recipient = await userManager.GetUserAsync(messageDto.RecipientId);

        if (sender == null)
        {
            return new ManagerResponse<MessageDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = ["A message must have a valid sender."]
            };
        }

        if (recipient == null )
        {
            return new ManagerResponse<MessageDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = ["A message must have a valid recipient."]
            };
        }

        if (sender.Id != userId)
        {
            return new ManagerResponse<MessageDto>()
            {
                ResponseType = ManagerResponseType.Forbidden
            };
        }
        
        if (userId == messageDto.RecipientId)
        {
            return new ManagerResponse<MessageDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = ["You cannot send a message to yourself."]
            };
        }        

        var message = new Message
        {
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = messageDto.Content,
            DateRead = messageDto.IsRead ? DateTime.UtcNow : null
        };

        context.Messages.Add(message);

        if (await context.SaveChangesAsync() == 0)
        {
            return new ManagerResponse<MessageDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = ["Failed to create new message."]
            };
        }

        return new ManagerResponse<MessageDto>(new MessageDto(message) { 
            SenderDisplayName = sender.DisplayName ?? "", 
            SenderProfileImageUrl = sender.ProfileImage?.Url,
            RecipientDisplayName = recipient.DisplayName ?? "" 
            });
    }    

    public async Task<ManagerResponse> DeleteMessage(long userId, long messageId)
    {
        var message = await context.Messages.FindAsync(messageId);
        if (message == null)
        {
            return new ManagerResponse<MessageDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"Message with id {messageId} was not found."]
            };            
        }

        if (message.SenderId != userId && message.RecipientId != userId)
        {
            
            return new ManagerResponse<MessageDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = ["Only the sender or receiver of a message may delete it."]
            };
        }        

        if (message.SenderId == userId)
        {
            message.SenderDeleted = true;
        }

        if (message.RecipientId == userId)
        {
            message.RecipientDeleted = true;
        }
        
        if (message.SenderDeleted && message.RecipientDeleted)
        {
            context.Messages.Remove(message);            
        }

        if (await context.SaveChangesAsync() == 0)
        {
            return new ManagerResponse<MessageDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = ["Failed to create new message."]
            };
        }

        return new ManagerResponse() { ResponseType = ManagerResponseType.Ok };
    }
    
    public async Task<IEnumerable<ContactDto>> GetContactsAsync(long userId)
    {
        return await context.Messages                    
            .Include(message => message.Sender)
                .ThenInclude(user => user.ProfileImage)
            .Include(message => message.Recipient)
                .ThenInclude(user => user.ProfileImage)
            .Where(message => message.SenderId == userId || message.RecipientId == userId)
            .GroupBy(message => message.SenderId == userId ? message.RecipientId : message.SenderId)
            .Select(group => group.OrderByDescending(m => m.DateSent)
                .Select(message => new ContactDto(message, message.SenderId == userId))
                .First()
            )
            .ToListAsync();
    }

    public async Task<ManagerResponse<MessageGroupDto>> AddMessageGroup (MessageGroup group)
    {
        context.MessageGroups.Add(group);
        if (await context.SaveChangesAsync() == 0)
        {
            return new ManagerResponse<MessageGroupDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages =  [$"Unable to add group with name {group.Name}"]
            };
        }

        return new ManagerResponse<MessageGroupDto>(new MessageGroupDto(group));
    }

    public async Task<ManagerResponse<MessageGroupDto>> AddConnectionToMessageGroup (string groupName, MessageConnection connection)
    {
        var group = await context.MessageGroups.FindAsync(groupName);

        if (group == null)
        {
            return new ManagerResponse<MessageGroupDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"Unable to find group with name {groupName}"]
            };
        }

        connection.MessageGroupName = group.Name;
        group.Connections.Add(connection);
        if (await context.SaveChangesAsync() == 0)
        {
            return new ManagerResponse<MessageGroupDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"Unable to add connection to group {groupName}"]
            };
        }

        return new ManagerResponse<MessageGroupDto>(new MessageGroupDto(group));
    }

    public async Task<bool> RemoveMessageConnection (string connectionId)
    {
        var connection = await context.MessageConnections.FindAsync(connectionId);
        if (connection == null)
        {
            return false;
        }
        
        context.MessageConnections.Remove(connection);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<MessageConnectionDto?> GetMessageConnection(string connectionId)
    {
        return await context.MessageConnections
            .Where(messageConnection => messageConnection.Id == connectionId)
            .Select(messageConnection => new MessageConnectionDto(messageConnection))
            .FirstOrDefaultAsync();
    }

    public async Task<MessageGroupDto?> GetMessageGroup(string groupName)
    {
        return await context.MessageGroups
            .Include(group => group.Connections)
            .Where(group => group.Name == groupName)
            .Select(group => new MessageGroupDto(group))
            .FirstOrDefaultAsync();
    }

    
    public async Task<MessageGroupDto?> GetMessageGroupForConnection(string connectionId)
    {
        return await context.MessageGroups
            .Where(group => group.Connections.Any(connection => connection.Id == connectionId))
            .Include(group => group.Connections)
            .Select(group => new MessageGroupDto(group))
            .FirstOrDefaultAsync();
    }
    
}