using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace LineConsole.Infrastructure.Identity;

/// <summary>
/// JWT 的簽章金鑰與發行資訊。簽發（JwtTokenGenerator）和驗證（JwtBearer）共用同一份，避免兩邊讀到不同的金鑰。
/// 金鑰不放在 appsettings.json：從環境變數 Jwt__Key 或 dotnet user-secrets 讀取。
/// </summary>
public sealed class JwtSettings
{
    /// <summary>HS256 至少要 256 bits</summary>
    public const int MinKeyBytes = 32;

    public required byte[] Key { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public TimeSpan Lifetime { get; init; } = TimeSpan.FromHours(2);

    /// <summary>沒有設定金鑰、自動產生的暫時金鑰（重開程式後舊的 token 會失效）</summary>
    public bool IsEphemeral { get; init; }

    /// <param name="allowEphemeral">沒設定金鑰時可不可以自動產生（開發環境、展示模式可以；正式環境不行）</param>
    public static JwtSettings FromConfiguration(IConfiguration configuration, bool allowEphemeral)
    {
        var section = configuration.GetSection("Jwt");
        var configured = section["Key"];
        byte[] key;
        var ephemeral = false;

        if (!string.IsNullOrWhiteSpace(configured))
        {
            key = Encoding.UTF8.GetBytes(configured);
            if (key.Length < MinKeyBytes)
                throw new InvalidOperationException($"Jwt:Key 太短：至少要 {MinKeyBytes} 個位元組（HS256）。");
        }
        else if (allowEphemeral)
        {
            key = RandomNumberGenerator.GetBytes(MinKeyBytes);
            ephemeral = true;
        }
        else
        {
            throw new InvalidOperationException(
                "缺少 Jwt:Key。請用環境變數 Jwt__Key 或 `dotnet user-secrets set Jwt:Key <金鑰>` 設定（至少 32 個字元）。");
        }

        return new JwtSettings
        {
            Key = key,
            Issuer = section["Issuer"] ?? "LineConsole",
            Audience = section["Audience"] ?? "LineConsoleUsers",
            IsEphemeral = ephemeral
        };
    }
}
