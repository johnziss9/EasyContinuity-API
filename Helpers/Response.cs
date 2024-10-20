namespace EasyContinuity_API.Helpers
{
    public class Response<T>
    {
        public int ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }

        public Response()
        {
        }

        public Response(T data)
        {
            Data = data;
        }

        public Response(int errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public Response(Exception ex)
        {
            ErrorCode = 500;
            ErrorMessage = ex.Message;
        }

        public bool IsSuccess => ErrorCode == 0 && Data != null;

        public static Response<T> Success(T data) => new(data);

        public static Response<T> Fail(int errorCode, string errorMessage) => new Response<T>(errorCode, errorMessage);

        public static Response<T> Fail(List<string?> errorMessages) => new Response<T> { ErrorCode = 500, ErrorMessage = string.Join(", ", errorMessages) };

        
        public static Response<T> Fail(Exception ex) => new Response<T>(ex);
    }
}