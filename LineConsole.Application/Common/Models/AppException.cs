namespace LineConsole.Application.Common.Models;

/// <summary>
/// 可攜帶錯誤代碼與訊息的應用層例外（預設對應 HTTP 400）
/// </summary>
public class AppException : Exception
{
    public string Code { get; }

    public AppException(string code, string message) : base(message)
    {
        Code = code;
    }
}

/// <summary>
/// 找不到資源，或資源不屬於目前的使用者（對應 HTTP 404）。
/// 不屬於自己的資源也回「找不到」，不透露資源是否存在。
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string code, string message) : base(code, message)
    {
    }
}
