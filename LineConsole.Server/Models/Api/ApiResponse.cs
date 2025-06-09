namespace LineConsole.Server.Models.Api;

/// <summary>標準 Web API 回傳格式，封裝業務代碼、訊息與資料內容</summary>
public record ApiResponse<T>
{
    public required string Code { get; init; } // 業務邏輯代碼，例如 SUCCESS、VALIDATION_ERROR、LINE_API_FAIL
    public required string Message { get; init; } // 使用者提示訊息，可支援多語系
    public T? Data { get; init; } // 實際回傳的資料（可能為 null 或空物件）

    /// <summary>操作成功，回傳資料</summary>
    public static ApiResponse<T> Success(T data, string message = "操作成功") =>
        new() { Code = "SUCCESS", Message = message, Data = data };

    /// <summary>操作失敗，回傳錯誤訊息與錯誤代碼</summary>
    public static ApiResponse<T> Fail(string code, string message) =>
        new() { Code = code, Message = message, Data = default };
}
