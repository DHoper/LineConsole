namespace LineConsole.Server.Models.Api;

public class ApiResponse<T>
{
    public bool Success { get; set; } // �O�_���\
    public T? Data { get; set; } // �^�Ǹ��
    public ApiError? Error { get; set; } // ���~��T�]�Y���\�h�� null�^

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
    public string Code { get; set; } = string.Empty; // ���~�N�X
    public string Message { get; set; } = string.Empty; // ���~�T��

    public ApiError() { }

    public ApiError(string code, string message)
    {
        Code = code;
        Message = message;
    }
}
