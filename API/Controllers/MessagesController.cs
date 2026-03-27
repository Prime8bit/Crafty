using System.Security.Claims;
using API.Data;
using API.Pagination;
using CraftyCommon.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController(IMessageManager messageManager) : BaseApiController
{
    [HttpGet("{messageId}")]
    public async Task<ActionResult<MessageDto>> GetMessage(long messageId)
    {        
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to get a message.");
        } 

        return GetActionResult(await messageManager.GetMessage(userId, messageId));
    }

    [HttpGet("thread/{recipientId}")]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessageThread(long recipientId, [FromQuery] PaginationParams paginationParams)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to get a message.");
        } 

        return GetActionResult(await messageManager.GetMessageThread(userId, recipientId, paginationParams));
    }
    
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto messageDto)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to create a message.");
        } 

        var result = await messageManager.AddMessage(userId, messageDto);
        
        // There is no "GetMessage" in this controller, so just return Ok instead of CreatedAtAction
        return CreatedAtAction(nameof(GetMessage), new { messageId = result.Data!.Id}, result.Data);
    }

    [HttpDelete("{messageId}")]
    public async Task<ActionResult> DeleteMessage(long messageId)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to delete a message.");
        } 

        return GetActionResult(await messageManager.DeleteMessage(userId, messageId));        
    }    

    [HttpGet("contacts")]
    public async Task<ActionResult<IEnumerable<ContactDto>>> GetContacts()
    {   
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to edit your user profile image.");
        }

        var result = await messageManager.GetContactsAsync(userId);
        return Ok(result);
    }
}