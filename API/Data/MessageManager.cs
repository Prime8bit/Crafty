using API.Data.Configuration;
using API.Entities;
using API.Misc;
using API.Pagination;
using CraftyCommon.DTOs;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageManager(DataContext context, UserManager<User> userManager) : IMessageManager
{
    public async Task<ManagerResponse<MessageDto>> GetMessage(long userId, long messageId)
    {

        var result = await context.Messages
            .Include(message => message.Sender!)
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
            
        return new ManagerResponse<MessageDto>(result.Adapt<MessageDto>());
    }

    public async Task<ManagerResponse<PagedList<MessageDto>>> GetMessageThread(long senderId, long recipientId, PaginationParams paginationParams)
    {        
        var sender = await userManager.FindByIdAsync(senderId.ToString());
        
        if (sender == null)
        {
            return new ManagerResponse<PagedList<MessageDto>>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"User with id {senderId} does not exist."]
            };
        }

        var recipient = await userManager.FindByIdAsync(recipientId.ToString());
        
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
            .Include(message => message.Sender!)
                .ThenInclude(user => user.ProfileImage)
            .Include(message => message.Recipient)
            .Where(message => (message.RecipientId == sender.Id && message.SenderId == recipient.Id)
                || (message.SenderId == sender.Id && message.RecipientId == recipient.Id))
            .OrderByDescending(message => message.DateSent)
            .ProjectToType<MessageDto>();

        var result = await PagedList<MessageDto>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);

        return new ManagerResponse<PagedList<MessageDto>>(result);
    }    

    public async Task<ManagerResponse<MessageDto>> AddMessage(long userId, CreateMessageDto messageDto)
    {        
        var sender = await userManager.FindByIdAsync(userId.ToString());
        var recipient = await userManager.FindByIdAsync(messageDto.RecipientId.ToString());

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
            Sender = sender,
            RecipientId = recipient.Id,
            Recipient = recipient,
            Content = messageDto.Content,
            DateSent = DateTime.Now,
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

        return new ManagerResponse<MessageDto>(message.Adapt<MessageDto>());
    }    

    public async Task<ManagerResponse> DeleteMessage(long userId, long messageId)
    {
        using (var transaction = await context.Database.BeginTransactionAsync())
        {
            try
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
                        ErrorMessages = ["Failed to delete message."]
                    };
                }

                await transaction.CommitAsync();

                return new ManagerResponse() { ResponseType = ManagerResponseType.Ok };
                
            } catch (Exception ex)
            {                
                return new ManagerResponse<MessageDto>()
                {
                    ResponseType = ManagerResponseType.BadRequest,
                    ErrorMessages = [ex.Message]
                }; 
            }
        }
    }
    
    public async Task<IEnumerable<ContactDto>> GetContactsAsync(long userId)
    {
        return await context.Messages                    
            .Include(message => message.Sender!)
                .ThenInclude(user => user.ProfileImage)
            .Include(message => message.Recipient!)
                .ThenInclude(user => user.ProfileImage)
            .Where(message => message.SenderId == userId || message.RecipientId == userId)
            .GroupBy(message => message.SenderId == userId ? message.RecipientId : message.SenderId)
            .Select(group => group.OrderByDescending(m => m.DateSent)
                .Select(message => new ContactDto {
                    Id = message.SenderId == userId ? message.RecipientId : message.SenderId,
                    DisplayName = message.SenderId == userId 
                        ? message.Recipient == null ? null : message.Recipient.DisplayName 
                        : message.Sender == null ? null : message.Sender.DisplayName,
                    ProfileImageUrl = message.SenderId == userId 
                        ? message.Recipient == null || message.Recipient.ProfileImage == null ? null : message.Recipient.ProfileImage.Url 
                        : message.Sender == null || message.Sender.ProfileImage == null ? null : message.Sender.ProfileImage.Url,
                    LastMessage = message.SenderId == userId ? "" : message.Content,
                    LastMessageDate = message.DateSent,
                    WasLastMessageRead = message.DateRead != null
                })
                .First()
            )
            .ToListAsync();
    }

    public async Task<ManagerResponse<MessageGroupDto>> AddConnectionToMessageGroup (string groupName, MessageConnection connection)
    {
        using (var transaction = await context.Database.BeginTransactionAsync())
        {
            try
            {
                var group = await context.MessageGroups.FindAsync(groupName);

                if (group == null)
                {
                    group = new MessageGroup() { Name = groupName };
                    context.MessageGroups.Add(group);
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

                await transaction.CommitAsync();

                return new ManagerResponse<MessageGroupDto>(group.Adapt<MessageGroupDto>());
            } catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ManagerResponse<MessageGroupDto>()
                {                    
                    ResponseType = ManagerResponseType.BadRequest,
                    ErrorMessages = [ex.Message]
                };
            }
        }
    }

    public async Task<ManagerResponse<MessageGroupDto?>> RemoveMessageConnection (string connectionId)
    {
        using (var transaction = await context.Database.BeginTransactionAsync())
        {
            try
            {
                var connection = await context.MessageConnections.FindAsync(connectionId);
                if (connection == null)
                {
                    return new ManagerResponse<MessageGroupDto?>()
                    {
                        ResponseType = ManagerResponseType.NotFound,
                        ErrorMessages = [$"Unable to find connection with id {connectionId}"]
                    };
                }
                
                var group = await context.MessageGroups.FindAsync(connection.MessageGroupName);
                if (group == null)
                {
                    return new ManagerResponse<MessageGroupDto?>()
                    {
                        ResponseType = ManagerResponseType.NotFound,
                        ErrorMessages = [$"Unable to find group associated with a connection with id {connectionId}"]
                    };
                }
                
                context.MessageConnections.Remove(connection);
                // If this is the last connection in its group, also delete the group.
                if (group.Connections.Count() == 1)
                {
                    context.MessageGroups.Remove(group);
                }

                if (await context.SaveChangesAsync() == 0)
                {
                    
                    return new ManagerResponse<MessageGroupDto?>()
                    {
                        ResponseType = ManagerResponseType.BadRequest,
                        ErrorMessages = [$"Unable to delete connection with id {connectionId}"]
                    };
                }

                await transaction.CommitAsync();

                return new ManagerResponse<MessageGroupDto?>(group.Adapt<MessageGroupDto>());
            } catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ManagerResponse<MessageGroupDto?>()
                {                    
                    ResponseType = ManagerResponseType.BadRequest,
                    ErrorMessages = [ex.Message]
                };
            }
        }
    }

    public async Task<MessageGroupDto?> GetMessageGroup(string groupName)
    {
        return await context.MessageGroups
            .Include(group => group.Connections)
            .Where(group => group.Name == groupName)
            .ProjectToType<MessageGroupDto>()
            .FirstOrDefaultAsync();
    }

    
    public async Task<MessageGroupDto?> GetMessageGroupForConnection(string connectionId)
    {
        return await context.MessageGroups
            .Where(group => group.Connections.Any(connection => connection.Id == connectionId))
            .Include(group => group.Connections)
            .ProjectToType<MessageGroupDto>()
            .FirstOrDefaultAsync();
    }
    
}