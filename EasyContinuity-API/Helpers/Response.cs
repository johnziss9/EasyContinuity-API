namespace EasyContinuity_API.Helpers
{
    public class Response<T>
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    private Response() { }

    private Response(T data)
    {
        StatusCode = 200;
        Data = data;
        Message = "Success";
    }

    private Response(int statusCode, string message, T? data = default)
    {
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }

    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

    public static Response<T> Success(T data) => new(data);

    public static Response<T> Fail(int statusCode, string message) 
        => new(statusCode, message);

    public static Response<T> NotFound(string message = "Not Found") 
        => new(404, message);

    public static Response<T> BadRequest(string message) 
        => new(400, message);

    public static Response<T> ValidationError(List<string> errors) 
        => new(422, string.Join(", ", errors));

    public static Response<T> InternalError(Exception ex) 
        => new(500, ex.Message);
}
}