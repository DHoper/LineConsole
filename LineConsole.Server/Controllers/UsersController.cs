using LineConsole.Application.Users.Models;
using LineConsole.Application.Users.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LineConsole.Server.Controllers;

/// <summary>
/// 提供後台使用者帳戶管理的 API
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserProfileService _userService;

    public UsersController(IUserProfileService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 註冊新使用者
    /// </summary>
    //[HttpPost]
    //public async Task<ActionResult<CreateUserProfileResult>> Register([FromBody] CreateUserProfileRequest request)
    //{
    //    var result = await _userService.RegisterAsync(request);
    //    return CreatedAtAction(nameof(GetByEmail), new { email = request.Email }, result);
    //}

    /// <summary>
    /// 根據 Email 查詢使用者
    /// </summary>
    //[HttpGet("{email}")]
    //public async Task<ActionResult<UserProfileDTO>> GetByEmail([FromRoute] string email)
    //{
    //    var user = await _userService.GetByEmailAsync(email);
    //    return user is not null ? Ok(user) : NotFound();
    //}

    /// <summary>
    /// 刪除使用者
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}
