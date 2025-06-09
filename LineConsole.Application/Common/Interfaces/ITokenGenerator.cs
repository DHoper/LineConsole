using LineConsole.Application.Common.Models;

namespace LineConsole.Application.Common.Interfaces;

/// <summary>
/// 定義產出 JWT Token 的功能介面
/// </summary>
public interface ITokenGenerator
{
    /// <summary>根據給定的使用者資訊產出 JWT Token 字串</summary>
    string GenerateToken(JwtPayload payload);
}
