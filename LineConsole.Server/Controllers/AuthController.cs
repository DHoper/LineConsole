using LineConsole.Application.Common.Interfaces;
using LineConsole.Application.Common.Models;
using LineConsole.Server.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LineConsole.Server.Controllers;

/// <summary>
/// 提供註冊與登入功能的 API 入口（使用 ASP.NET Core Identity）
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAccountManager _accountManager;

    public AuthController(IAccountManager accountManager)
    {
        _accountManager = accountManager;
    }

    /// <summary>
    /// 使用者註冊（建立 IdentityUser、UserProfiles 與 LineOfficialAccount）
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<string>>> RegisterAsync([FromBody] RegisterInput request)
    {
        try
        {
            var userId = await _accountManager.RegisterAsync(request);
            return ApiResponse<string>.Success(userId);
        }
        catch (AppException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Code, ex.Message));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Register] 伺服器錯誤: {ex}");
            return StatusCode(500, ApiResponse<string>.Fail("SERVER_ERROR", "伺服器發生錯誤"));
        }
    }

    /// <summary>
    /// 使用者登入並取得 JWT Token
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<string>>> LoginAsync([FromBody] LoginInput request)
    {
        try
        {
            var token = await _accountManager.LoginAsync(request.Email, request.Password);
            return ApiResponse<string>.Success(token);
        }
        catch (AppException ex)
        {
            return Unauthorized(ApiResponse<string>.Fail(ex.Code, ex.Message));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Login] 伺服器錯誤: {ex}");
            return StatusCode(500, ApiResponse<string>.Fail("SERVER_ERROR", "伺服器發生錯誤"));
        }
    }
}
