namespace LineConsole.Server.Models.Api;

/// <summary>
/// 可攜帶錯誤代碼與訊息的應用層例外
/// </summary>
public class AppException : Exception
{
    public string Code { get; }

    public AppException(string code, string message) : base(message)
    {
        Code = code;
    }
}