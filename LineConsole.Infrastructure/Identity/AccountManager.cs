using LineConsole.Application.Common.Interfaces;
using LineConsole.Application.Common.Models;
using LineConsole.Application.UserProfiles.Interfaces;
using LineConsole.Application.UserProfiles.Models;
using LineConsole.Domain.Entities;
using LineConsole.Server.Models.Api;
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
    /// ���U�s�b���]�إ� IdentityUser + UserProfiles + LINE �x��b���^
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
            await _userManager.DeleteAsync(user); // �^�u
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
            await _userManager.DeleteAsync(user); // �^�u
            throw;
        }
        catch (Exception ex)
        {
            await _userManager.DeleteAsync(user); // �^�u
            throw new AppException("USER_PROFILE_FAILED", $"���U�ӤH��Ʈɵo�Ϳ��~�G{ex.Message}");
        }

        return user.Id;
    }


    /// <summary>
    /// �n�J�æ^�� JWT Token
    /// </summary>
    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            throw new AppException("LOGIN_FAILED", "�b�����s�b");

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
            throw new AppException("LOGIN_FAILED", "�K�X���~");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? RoleNames.User;

        var token = _tokenGenerator.GenerateToken(new JwtPayload
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = role
        });

        return token;
    }
}
