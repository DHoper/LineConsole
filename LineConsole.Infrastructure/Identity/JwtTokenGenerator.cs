using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using LineConsole.Application.Common.Interfaces;
using LineConsole.Infrastructure.Identity;
using AppJwtPayload = LineConsole.Application.Common.Models.JwtPayload;

namespace LineConsole.Infrastructure.Security;

/// <summary>
/// 使用 JWT 標準產生 Token，嵌入角色與用戶資訊
/// </summary>
public class JwtTokenGenerator : ITokenGenerator
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(JwtSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// 產生 JWT 並回傳 Token 與過期時間（Unix 秒）
    /// </summary>
    public (string Token, long ExpiresAt) GenerateToken(AppJwtPayload payload)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, payload.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, payload.Email),
            new(ClaimTypes.Role, payload.Role)
        };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(_settings.Key), SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.Add(_settings.Lifetime);
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var expiresAt = new DateTimeOffset(expires).ToUnixTimeSeconds();

        return (tokenString, expiresAt);
    }
}
