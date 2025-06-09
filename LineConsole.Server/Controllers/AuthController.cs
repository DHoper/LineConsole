using LineConsole.Application.Common.Interfaces;
using LineConsole.Application.Common.Models;
using LineConsole.Server.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace LineConsole.Server.Controllers;

/// <summary>
/// 提供註冊與登入功能的 API 入口（使用 ASP.NET Core Identity）
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAccountManager _accountManager;

    public AuthController(IAccountManager accountManager)
    {
        _accountManager = accountManager;
    }

    /// <summary>
    /// 使用者註冊（建立 IdentityUser 與 UserProfile）
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<string>>> RegisterAsync([FromBody] RegisterRequest request)
    {
        var userId = await _accountManager.RegisterAsync(request.Email, request.Password);
        return ApiResponse<string>.Success(userId.ToString()); // ✅ 注意 Guid 轉字串
    }

    /// <summary>
    /// 使用者登入並取得 JWT Token
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<string>>> LoginAsync([FromBody] LoginRequest request)
    {
        var token = await _accountManager.LoginAsync(request.Email, request.Password);
        if (token is null)
            return Unauthorized(ApiResponse<string>.Fail("LOGIN_FAILED", "帳號或密碼錯誤")); // ✅ 修正

        return ApiResponse<string>.Success(token);
    }
}
