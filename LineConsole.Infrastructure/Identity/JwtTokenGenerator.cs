using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LineConsole.Application.Common.Interfaces;
using AppJwtPayload = LineConsole.Application.Common.Models.JwtPayload;

namespace LineConsole.Infrastructure.Security;

/// <summary>
/// 使用 JWT 標準產生 Token，嵌入角色與用戶資訊
/// </summary>
public class JwtTokenGenerator : ITokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
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

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddHours(2); // 自訂過期時間
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var expiresAt = new DateTimeOffset(expires).ToUnixTimeSeconds(); // ? 轉為 Unix 秒

        return (tokenString, expiresAt);
    }
}
