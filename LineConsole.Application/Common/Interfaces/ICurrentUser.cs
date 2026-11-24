namespace LineConsole.Application.Common.Interfaces;

/// <summary>
/// 目前登入的使用者。由 Server 層從 JWT 取出，應用層用它做授權判斷，不直接碰 HttpContext。
/// </summary>
public interface ICurrentUser
{
    /// <summary>ASP.NET Identity 的使用者 ID；沒有登入時是 null</summary>
    string? IdentityUserId { get; }
}
