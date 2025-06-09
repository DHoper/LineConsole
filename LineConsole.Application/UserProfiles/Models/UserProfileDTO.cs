namespace LineConsole.Application.Users.Models;

/// <summary>��x�ϥΪ��X�R��ơ]�ѥ~���^�ǡ^</summary>
public record class UserProfileDTO
{
    /// <summary>�X�R��ƥD�� ID</summary>
    public Guid Id { get; init; }

    /// <summary>Identity �b�� ID</summary>
    public string IdentityUserId { get; init; } = string.Empty;

    /// <summary>�ϥΪ���ܦW�١]�i��^</summary>
    public string? DisplayName { get; init; }

    /// <summary>�ϥΪ̤j�Y�K���}�]�i��^</summary>
    public string? AvatarUrl { get; init; }

    /// <summary>��´�N�X�]�i��^</summary>
    public string? OrganizationCode { get; init; }

    /// <summary>�إ߮ɶ��]UTC�^</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>�̫��s�ɶ��]UTC�^</summary>
    public DateTime UpdatedAt { get; init; }
}
