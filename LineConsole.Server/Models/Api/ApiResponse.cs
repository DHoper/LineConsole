namespace LineConsole.Server.Models.Api;

public class ApiResponse<T>
{
    public bool Success { get; set; } // 是否成功
    public T? Data { get; set; } // 回傳資料
    public ApiError? Error { get; set; } // 錯誤資訊（若成功則為 null）

    public static ApiResponse<T> SuccessResponse(T data) => new()
    {
        Success = true,
        Data = data,
        Error = null
    };

    public static ApiResponse<T> FailResponse(string code, string message) => new()
    {
        Success = false,
        Data = default,
        Error = new ApiError(code, message)
    };
}

public class ApiError
{
    public string Code { get; set; } = string.Empty; // 錯誤代碼
    public string Message { get; set; } = string.Empty; // 錯誤訊息

    public ApiError() { }

    public ApiError(string code, string message)
    {
        Code = code;
        Message = message;
    }
}
