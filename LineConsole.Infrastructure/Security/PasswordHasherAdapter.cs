using LineConsole.Application.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace LineConsole.Infrastructure.Security;

/// <summary>
/// �ʸ� ASP.NET Core Identity ���K�X���꾹
/// </summary>
public class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(null!, password);
    }

    public bool Verify(string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
