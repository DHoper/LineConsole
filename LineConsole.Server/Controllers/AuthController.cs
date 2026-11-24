using LineConsole.Application.Common.Interfaces;
using LineConsole.Application.Common.Models;
using LineConsole.Server.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LineConsole.Server.Controllers;

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

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<string>>> RegisterAsync([FromBody] RegisterInput request)
    {
        try
        {
            var userId = await _accountManager.RegisterAsync(request);
            return ApiResponse<string>.SuccessResponse(userId);
        }
        catch (AppException ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse(ex.Code, ex.Message));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResult>>> LoginAsync([FromBody] LoginInput request)
    {
        try
        {
            var result = await _accountManager.LoginAsync(request.Email, request.Password);
            return ApiResponse<LoginResult>.SuccessResponse(result);
        }
        catch (AppException ex)
        {
            return Unauthorized(ApiResponse<LoginResult>.FailResponse(ex.Code, ex.Message));
        }
    }
}
