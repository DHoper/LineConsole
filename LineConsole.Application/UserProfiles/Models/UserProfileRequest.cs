namespace LineConsole.Application.Users.Models;

/// <summary>���U��x�ϥΪ��X�R��ƪ��ШD���</summary>
public record class CreateUserProfileRequest
{
    /// <summary>������ Identity �ϥΪ� ID</summary>
    public string IdentityUserId { get; init; } = string.Empty;
}