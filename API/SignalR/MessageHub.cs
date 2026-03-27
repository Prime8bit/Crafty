using API.Data;
using API.Entities;
using API.Misc;
using API.Pagination;
using CraftyCommon.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public enum MessageHubMessages
{
    SendMessage,
    ReceiveMessageThread,
    UpdatedGroup,
    DeleteMessage
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
        if (groupDto != null)
        {
            await Clients.Group(groupDto.Name).SendAsync(MessageHubMessages.UpdatedGroup.ToString());
        }
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

    public async Task DeleteMessage(long messageId)
    {
        var senderIdStr = Context.UserIdentifier;
        long senderId;
        var group = await messageManager.GetMessageGroupForConnection(Context.ConnectionId);

        if (string.IsNullOrEmpty(senderIdStr) 
            || long.TryParse(senderIdStr, out senderId) == false
            || group == null)
        {
            throw new MessageHubException("You must be logged in to delete a message.");
        }

        var response = await messageManager.DeleteMessage(senderId, messageId);
        
        // A delete technically succeeds if there is nothing to delete so NotFound is ok.
        if (response.ResponseType != ManagerResponseType.Ok 
            && response.ResponseType != ManagerResponseType.NotFound)
        {
            throw new MessageHubException($"Message deletion failed:\n{string.Join('\n', response.ErrorMessages)}");
        }

        await Clients.Caller.SendAsync(MessageHubMessages.DeleteMessage.ToString(), messageId);
    }

    private string GetGroupName(long senderId, long recipientId)
    {
        return senderId < recipientId ? $"{senderId}-{recipientId}" : $"{recipientId}-{senderId}";
    }

    private async Task<MessageGroupDto> AddToGroup(string groupName)
    {
        long userId;
        if (Context.UserIdentifier == null || !long.TryParse(Context.UserIdentifier, out userId))
        {
            throw new Exception ("Unable to get user id.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var connection = new MessageConnection{ 
            Id = Context.ConnectionId, 
            MessageGroupName = groupName,
            UserId = userId 
            };
            
        var updateGroupResponse = await messageManager.AddConnectionToMessageGroup(groupName, connection);

        if (updateGroupResponse.Data == null)
        {
            throw new HubException($"Failed to join group {groupName}.");
        }

        return updateGroupResponse.Data;
    }
    
    private async Task<MessageGroupDto?> RemoveFromMessageGroup()
    {
        var groupResponse = await messageManager.RemoveMessageConnection(Context.ConnectionId);
        // It is ok if the group response fails from a NotFound error. This is likely due to a refresh of the browser
        // page causing multiple disconnect requests simultaneously.
        if (groupResponse.ResponseType != ManagerResponseType.Ok 
            && groupResponse.ResponseType != ManagerResponseType.NotFound)
        {
            throw new HubException(string.Join('\n', groupResponse.ErrorMessages));
        }

        return groupResponse.Data;
    }
}