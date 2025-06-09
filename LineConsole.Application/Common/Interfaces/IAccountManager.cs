namespace LineConsole.Application.Common.Interfaces;

/// <summary>
/// �w�q�b�����������μh�ާ@�]���U�P�n�J�^
/// </summary>
public interface IAccountManager
{
    /// <summary>���U�s�b��</summary>
    /// <param name="email">�ϥΪ� Email</param>
    /// <param name="password">�n�J�K�X</param>
    /// <param name="accountType">�b�������]UserProfile / Admin�^</param>
    /// <returns>�إߦ��\���ϥΪ� ID</returns>
    Task<string> RegisterAsync(string email, string password, string accountType = "UserProfile");

    /// <summary>�b���n�J�A���\�h�^�� JWT Token</summary>
    /// <param name="email">�b�� Email</param>
    /// <param name="password">�b���K�X</param>
    /// <returns>JWT Token �� null�]�n�J���ѡ^</returns>
    Task<string?> LoginAsync(string email, string password);
}
