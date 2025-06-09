using LineConsole.Application.Common.Models;

namespace LineConsole.Application.Common.Interfaces;

/// <summary>
/// �w�q���X JWT Token ���\�श��
/// </summary>
public interface ITokenGenerator
{
    /// <summary>�ھڵ��w���ϥΪ̸�T���X JWT Token �r��</summary>
    string GenerateToken(JwtPayload payload);
}
