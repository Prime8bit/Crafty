using API.Misc;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    /** In ASP.NET Core, using the suffix "Async" in function names can break some of the reflection
    * resolution in some ASP.NET functionality. If you use "Async" at the end of the functions they may work
    * for a long time until you use specific ASP.NET functionality, like CreatedAtAction
    * https://github.com/dotnet/aspnetcore/issues/4849
    **/

    /** Note about sanitization
    * I know that I should add more error checking to my controllers and repositories for a production-level service
    * to prevent bad actors from using the API without my client to do malicious things. I decided against it because
    * this isn't a production service and I didn't want to flood my code with error checking. I didn't think it would be helpful
    **/

    public ActionResult<T> GetActionResult<T>(ManagerResponse<T> response)
    {
        // If it's an Ok response with data, return Ok(data)
        if (response.ResponseType == ManagerResponseType.Ok && response.Data != null)
        {
            return Ok(response.Data);
        }

        // Otherwise, let the base method handle the status code
        return GetActionResult((ManagerResponse)response);
    }

    public ActionResult GetActionResult(ManagerResponse response)
    {
        switch (response.ResponseType)
        {
            case ManagerResponseType.Ok:
                return Ok();
            case ManagerResponseType.NoContent:
                return NoContent();
            case ManagerResponseType.Unauthorized:
                return Unauthorized(response.ErrorMessages);
            case ManagerResponseType.Forbidden:
                return Forbid();
            case ManagerResponseType.NotFound:
                return NotFound(response.ErrorMessages);
            case ManagerResponseType.Conflict:
                return Conflict(response.ErrorMessages);
            default:
                return BadRequest(response.ErrorMessages);
        }
    }
}
