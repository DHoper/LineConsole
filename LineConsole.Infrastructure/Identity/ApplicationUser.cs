using Microsoft.AspNetCore.Identity;

namespace LineConsole.Infrastructure.Identity;

/// <summary>
/// �X�R ASP.NET Identity ���ϥΪ̼ҫ��A�䴩�e��x�@�αb���[�c
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>�b�������GUser = �@��ϥΪ̡AAdmin = �޲z��</summary>
    public string AccountType { get; set; } = "User";
}
