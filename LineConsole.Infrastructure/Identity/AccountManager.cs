using LineConsole.Application.Common.Interfaces;
using LineConsole.Application.Common.Models;
using LineConsole.Application.UserProfiles.Interfaces;
using LineConsole.Application.UserProfiles.Models;
using LineConsole.Domain.Entities;
using LineConsole.Server.Models.Api;
using Microsoft.AspNetCore.Identity;

namespace LineConsole.Infrastructure.Identity;

/// <summary>
/// 使用 ASP.NET Core Identity 實作帳號註冊與登入功能
/// </summary>
public class AccountManager : IAccountManager
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IUserProfileService _userProfileService;

    public AccountManager(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenGenerator tokenGenerator,
        IUserProfileService userProfileService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenGenerator = tokenGenerator;
        _userProfileService = userProfileService;
    }

    /// <summary>
    /// 註冊新帳號（建立 IdentityUser + UserProfiles + LINE 官方帳號）
    /// </summary>
    public async Task<string> RegisterAsync(RegisterInput request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            AccountType = RoleNames.User
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var message = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new AppException("REGISTER_FAILED", message);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, RoleNames.User);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user); // 回滾
            var message = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            throw new AppException("ROLE_ASSIGN_FAILED", message);
        }

        try
        {
            await _userProfileService.RegisterAsync(new CreateUserProfileInput
            {
                IdentityUserId = user.Id,
                DisplayName = request.DisplayName,
                LineAccount = request.LineAccount
            });
        }
        catch (AppException)
        {
            await _userManager.DeleteAsync(user); // 回滾
            throw;
        }
        catch (Exception ex)
        {
            await _userManager.DeleteAsync(user); // 回滾
            throw new AppException("USER_PROFILE_FAILED", $"註冊個人資料時發生錯誤：{ex.Message}");
        }

        return user.Id;
    }

    /// <summary>
    /// 登入並回傳 JWT Token 與使用者資訊
    /// </summary>
    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            throw new AppException("LOGIN_FAILED", "帳號不存在");

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
            throw new AppException("LOGIN_FAILED", "密碼錯誤");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? RoleNames.User;

        var profile = await _userProfileService.GetDetailsByIdentityUserIdAsync(user.Id);
        if (profile is null)
            throw new AppException("LOGIN_FAILED", "找不到對應的使用者資料");

        var payload = new JwtPayload
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = role
        };

        // ⚠ 改這裡：同時取得 token 與 expiresAt
        var (token, expiresAt) = _tokenGenerator.GenerateToken(payload);

        return new LoginResult
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new LoginUserInfo
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Role = role,
                DisplayName = profile.DisplayName,
                LineAccounts = profile.LineAccounts
            }
        };
    }
}
