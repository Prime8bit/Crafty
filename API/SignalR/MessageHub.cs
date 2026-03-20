using API.Data;
using API.DTOs;
using API.Entities;
using API.Misc;
using API.Pagination;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public enum MessageHubMessages
{
    SendMessage,
    ReceiveMessageThread,
    UpdatedGroup
}

public class MessageHubException : Exception
{
        public MessageHubException(string message) : base(message) {}
}

public class MessageHub(IMessageManager messageManager) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var recipientUserId = httpContext?.Request.Query["user"];

        var senderIdStr = Context.UserIdentifier;

        if (string.IsNullOrEmpty(senderIdStr) || string.IsNullOrEmpty(recipientUserId))
        {
            throw new MessageHubException("Cannot join group");
        }

        long senderId, recipientId;
        if (!long.TryParse(senderIdStr, out senderId) || !long.TryParse(recipientUserId, out recipientId))
        {
            throw new MessageHubException("Unable to parse user ids.");
        }

        var groupName = GetGroupName(senderId, recipientId);
        var groupDto = await AddToGroup(groupName);

        await Clients.Caller.SendAsync(MessageHubMessages.UpdatedGroup.ToString(), groupDto);

        var paginationParams = new PaginationParams() { 
            PageSize = 20,
            PageNumber = 1
        };

        var messageResponse = await messageManager.GetMessageThread(senderId, recipientId, paginationParams);
        if (messageResponse.Data == null)
        {
            throw new MessageHubException($"Failed to obtain message thread for users with ids {senderId} and {recipientId}.");
        }
        await Clients.Group(groupName).SendAsync(MessageHubMessages.ReceiveMessageThread.ToString(), messageResponse.Data);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var groupDto = await RemoveFromMessageGroup();
        await Clients.Group(groupDto.Name).SendAsync(MessageHubMessages.UpdatedGroup.ToString());
        await base.OnDisconnectedAsync(exception);
    }

    // Because this is a client facing endpoint, I don't call it SendMessageAsync because that is confusing to the client devs.
    public async Task SendMessage(CreateMessageDto messageDto)
    {
        var senderIdStr = Context.UserIdentifier;
        long senderId;

        if (string.IsNullOrEmpty(senderIdStr) || long.TryParse(senderIdStr, out senderId) == false)
        {
            throw new MessageHubException("You must be logged in to send a message.");
        }

        var groupName = GetGroupName(senderId, messageDto.RecipientId);
        var group = await messageManager.GetMessageGroup(groupName);

        // If the recipient is currently connected, assume they have read it. For simplicity's sake, I am not doing a handshake to verify.
        if (group != null && group.Connections.Any(connection => connection.UserId == messageDto.RecipientId))
        {
            messageDto.IsRead = true;
        }

        var response = await messageManager.AddMessage(senderId, messageDto);
        if (response.Data == null)
        {
            var errorStr = string.Join('\n', response.ErrorMessages);
            throw new MessageHubException($"Message creation failed:\n{errorStr}");
        }

        await Clients.Group(groupName).SendAsync(MessageHubMessages.SendMessage.ToString(), response.Data);
    }

    private string GetGroupName(long senderId, long recipientId)
    {
        return senderId < recipientId ? $"{senderId}-{recipientId}" : $"{recipientId}-{senderId}";
    }

    private async Task<MessageGroupDto> AddToGroup(string groupName)
    {        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        long userId;
        if (Context.UserIdentifier == null || !long.TryParse(Context.UserIdentifier, out userId))
        {
            throw new Exception ("Unable to get user id.");
        }
        var connection = new MessageConnection{ 
            Id = Context.ConnectionId, 
            MessageGroupName = groupName,
            UserId = userId 
            };

        var updateGroupResponse = await messageManager.AddConnectionToMessageGroup(groupName, connection);

        if (updateGroupResponse.Data != null)
        {
            return updateGroupResponse.Data;
        }
        else if (updateGroupResponse.ResponseType == ManagerResponseType.NotFound)
        {            
            var group = new MessageGroup { Name = groupName };
            group.Connections.Add(connection);
            var response = await messageManager.AddMessageGroup(group);
            if (response.Data != null)
            {
                return response.Data;
            }
        }

        throw new HubException($"Failed to join group {groupName}.");
    }
    
    private async Task<MessageGroupDto> RemoveFromMessageGroup()
    {
        var group = await messageManager.GetMessageGroupForConnection(Context.ConnectionId);
        var connection = group?.Connections.FirstOrDefault(connection => connection.Id == Context.ConnectionId);
        if (connection != null 
            && group != null 
            && await messageManager.RemoveMessageConnection(connection.Id))
        {
            return group;
        }

        throw new HubException("Failed to remove from group.");
    }
}