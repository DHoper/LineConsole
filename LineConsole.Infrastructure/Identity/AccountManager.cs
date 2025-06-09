using LineConsole.Application.Common.Interfaces;
using LineConsole.Application.Common.Models;
using LineConsole.Application.Users.Interfaces;
using LineConsole.Application.Users.Models;
using Microsoft.AspNetCore.Identity;

namespace LineConsole.Infrastructure.Identity;

/// <summary>
/// �ϥ� ASP.NET Core Identity ��@�b�����U�P�n�J�\��
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
    /// ���U�s�b���]�]�t IdentityUser �P UserProfile �X�R��ơ^
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

        // �إ��X�R��� UserProfile
        await _userProfileService.RegisterAsync(new CreateUserProfileRequest
        {
            IdentityUserId = user.Id
        });

        return user.Id;
    }

    /// <summary>
    /// �n�J�æ^�� JWT Token
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
