namespace API.Misc;

/** I know this requires my Services to know about http error codes, but the alternative is returning either a huge enum with lots of error values
 * or a lot of little enums with the same values then having a huge switch statement in the controller for mapping enum to HTTP Error codes
 * Since i have no intention of making any other contexts other than HTTP, this seemed the least dirty solution.
 */
public enum ManagerResponseType
{
    None,
    Ok,
    NoContent,
    BadRequest,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict
}
public class ManagerResponse<T> : ManagerResponse
{
    public T? Data { get; set; }

    public ManagerResponse() {}

    public ManagerResponse(T data)
    {
        Data = data;
        ResponseType = ManagerResponseType.Ok;
    }
}

public class ManagerResponse
{    
    public ManagerResponseType ResponseType { get; set; } = ManagerResponseType.None;
    public IEnumerable<string> ErrorMessages { get; set; } = new List<string>();
}