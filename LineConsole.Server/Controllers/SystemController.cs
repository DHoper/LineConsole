using LineConsole.Infrastructure;
using LineConsole.Infrastructure.Demo;
using LineConsole.Server.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LineConsole.Server.Controllers;

/// <summary>系統資訊：前端用來判斷是不是展示模式（顯示展示帳號、上方提示條）</summary>
[ApiController]
[Route("api/system")]
[AllowAnonymous]
public class SystemController : ControllerBase
{
    private readonly RuntimeInfo _runtime;

    public SystemController(RuntimeInfo runtime)
    {
        _runtime = runtime;
    }

    [HttpGet("info")]
    public ApiResponse<SystemInfo> GetInfo() => ApiResponse<SystemInfo>.SuccessResponse(new SystemInfo(
        _runtime.Mode.ToString(),
        _runtime.Mode == AppMode.Demo ? new DemoAccount(DemoData.Email, DemoData.Password) : null));
}

/// <param name="Mode">Demo 或 Live</param>
/// <param name="DemoAccount">展示模式才有：公開的展示帳號</param>
public sealed record SystemInfo(string Mode, DemoAccount? DemoAccount);

public sealed record DemoAccount(string Email, string Password);
