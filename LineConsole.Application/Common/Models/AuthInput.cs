using LineConsole.Application.LineOfficialAccounts.Models;

namespace LineConsole.Application.Common.Models;

/// <summary>
/// �ϥΪ̵��U��ơ]�Ω���U Identity�BUserProfiles �P LINE �x��b���^
/// </summary>
public class RegisterInput
{
    /// <summary>Email�A�N�@���n�J�b��</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>�K�X</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>�i��G��ܦW��</summary>
    public string? DisplayName { get; set; }

    /// <summary>�Y���U�ɻݸj�w LINE �x��b���A��g�����</summary>
    public LineOfficialAccountCreateModel LineAccount { get; set; } = new();
}


public record class LoginInput
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}