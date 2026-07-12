namespace Helios.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T> { Success = true, Data = data, Message = message };
    }

    public static ApiResponse<T> Fail(string error)
    {
        return new ApiResponse<T> { Success = false, Errors = new List<string> { error } };
    }

    public static ApiResponse<T> Fail(List<string> errors)
    {
        return new ApiResponse<T> { Success = false, Errors = errors };
    }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public static ApiResponse Ok(string? message = null)
    {
        return new ApiResponse { Success = true, Message = message };
    }

    public static ApiResponse Fail(string error)
    {
        return new ApiResponse { Success = false, Errors = new List<string> { error } };
    }

    public static ApiResponse Fail(List<string> errors)
    {
        return new ApiResponse { Success = false, Errors = errors };
    }
}
