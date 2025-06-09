namespace LineConsole.Application.Common.Interfaces;

/// <summary>
/// 定義帳號相關的應用層操作（註冊與登入）
/// </summary>
public interface IAccountManager
{
    /// <summary>註冊新帳號</summary>
    /// <param name="email">使用者 Email</param>
    /// <param name="password">登入密碼</param>
    /// <param name="accountType">帳號類型（UserProfile / Admin）</param>
    /// <returns>建立成功的使用者 ID</returns>
    Task<string> RegisterAsync(string email, string password, string accountType = "UserProfile");

    /// <summary>帳號登入，成功則回傳 JWT Token</summary>
    /// <param name="email">帳號 Email</param>
    /// <param name="password">帳號密碼</param>
    /// <returns>JWT Token 或 null（登入失敗）</returns>
    Task<string?> LoginAsync(string email, string password);
}
