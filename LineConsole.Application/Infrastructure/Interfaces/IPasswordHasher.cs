namespace LineConsole.Application.Infrastructure.Interfaces;

/// <summary>
/// ���ѱK�X����P���Ҫ�����
/// </summary>
public interface IPasswordHasher
{
    /// <summary>�N����K�X�ഫ�������</summary>
    string Hash(string password);

    /// <summary>���ҨϥΪ̿�J���K�X�O�_�ŦX�����</summary>
    bool Verify(string hashedPassword, string providedPassword);
}
