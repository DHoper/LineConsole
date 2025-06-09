using Microsoft.AspNetCore.Identity;

namespace LineConsole.Infrastructure.Identity;

/// <summary>
/// 擴充 ASP.NET Identity 的使用者模型，支援前後台共用帳號架構
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>帳號類型：User = 一般使用者，Admin = 管理員</summary>
    public string AccountType { get; set; } = "User";
}
