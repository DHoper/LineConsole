namespace LineConsole.Application.Common.Models;

/// <summary>
/// �w�q���X JWT Token �ɩһݪ��ϥΪ̸�Ƥ��e
/// </summary>
public record JwtPayload
{
    /// <summary>�ϥΪ��ѧO�X</summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>�ϥΪ� Email</summary>
    public string Email { get; init; } = string.Empty;
     
    /// <summary>�ϥΪ̩��ݨ���</summary>
    public string Role { get; init; } = string.Empty;
}
