namespace Contracts;

public class ApiResponse
{
    public object? Data { get; set; }
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? ErrorDetails { get; set; }

    public static ApiResponse Success(object data, string? message = null)
        => new ApiResponse { Data = data, IsSuccess = true, Message = message, StatusCode = 200 };

    public static ApiResponse Failure(string message, string? errorDetails = null)
        => new ApiResponse { IsSuccess = false, Message = message, ErrorDetails = errorDetails, StatusCode = 500 };
}

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? ErrorDetails { get; set; }
    public static ApiResponse<T> Success(T data, string? message = null)
        => new ApiResponse<T> { Data = data, IsSuccess = true, Message = message, StatusCode = 200 };

    public static ApiResponse<T> Failure(string message, string? errorDetails = null)
        => new ApiResponse<T> { IsSuccess = false, Message = message, ErrorDetails = errorDetails, StatusCode = 500 };
}
