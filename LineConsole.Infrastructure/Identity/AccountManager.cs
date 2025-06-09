using LineConsole.Application.Common.Interfaces;
using LineConsole.Application.Common.Models;
using LineConsole.Application.Users.Interfaces;
using LineConsole.Application.Users.Models;
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
    /// 註冊新帳號（包含 IdentityUser 與 UserProfile 擴充資料）
    /// </summary>
    public async Task<string> RegisterAsync(string email, string password, string accountType = "User")
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            AccountType = accountType
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, accountType);

        // 建立擴充資料 UserProfile
        await _userProfileService.RegisterAsync(new CreateUserProfileRequest
        {
            IdentityUserId = user.Id
        });

        return user.Id;
    }

    /// <summary>
    /// 登入並回傳 JWT Token
    /// </summary>
    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return null;

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "UserProfile";

        var token = _tokenGenerator.GenerateToken(new JwtPayload
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = role
        });

        return token;
    }
}
