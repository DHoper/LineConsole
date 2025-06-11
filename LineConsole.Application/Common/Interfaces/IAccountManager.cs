using LineConsole.Application.Common.Models;
using LineConsole.Application.UserProfiles.Models;

namespace LineConsole.Application.Common.Interfaces;

/// <summary>
/// �w�q�b�����������μh�ާ@�]���U�P�n�J�^
/// </summary>
public interface IAccountManager
{
    /// <summary>
    /// ���U�s�b���]�]�t�j�w LINE �x��b���^
    /// </summary>
    /// <param name="request">���U��T�]Email�B�K�X�BLINE channel ��T���^</param>
    /// <returns>�إߦ��\���ϥΪ� ID</returns>
    Task<string> RegisterAsync(RegisterInput request);

    /// <summary>
    /// �b���n�J�A���\�h�^�� JWT Token �P�ϥΪ̸�T
    /// </summary>
    /// <param name="email">�b�� Email</param>
    /// <param name="password">�b���K�X</param>
    /// <returns>�n�J���G�]�]�t JWT Token �P�ϥΪ̸�T�^</returns>
    Task<LoginResult> LoginAsync(string email, string password);
}
