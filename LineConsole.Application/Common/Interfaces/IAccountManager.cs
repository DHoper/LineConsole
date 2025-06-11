using LineConsole.Application.Common.Models;
using LineConsole.Application.UserProfiles.Models;

namespace LineConsole.Application.Common.Interfaces;

/// <summary>
/// 定義帳號相關的應用層操作（註冊與登入）
/// </summary>
public interface IAccountManager
{
    /// <summary>
    /// 註冊新帳號（包含綁定 LINE 官方帳號）
    /// </summary>
    /// <param name="request">註冊資訊（Email、密碼、LINE channel 資訊等）</param>
    /// <returns>建立成功的使用者 ID</returns>
    Task<string> RegisterAsync(RegisterInput request);

    /// <summary>
    /// 帳號登入，成功則回傳 JWT Token 與使用者資訊
    /// </summary>
    /// <param name="email">帳號 Email</param>
    /// <param name="password">帳號密碼</param>
    /// <returns>登入結果（包含 JWT Token 與使用者資訊）</returns>
    Task<LoginResult> LoginAsync(string email, string password);
}
