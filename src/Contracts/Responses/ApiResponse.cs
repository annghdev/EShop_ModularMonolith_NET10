namespace Contracts;

public class ApiResponse<T>
{
    public T Data { get; set; }
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }

    public static ApiResponse<T> Success(T data, string? message = null) =>
        new ApiResponse<T> { Data = data, IsSuccess = true, Message = message };

    public static ApiResponse<T> Failure(string message) => new ApiResponse<T> { IsSuccess = false, Message = message };
}
