using LineConsole.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;

namespace LineConsole.Tests.Security;

/// <summary>
/// JWT 金鑰不再寫在 appsettings.json：要從設定（環境變數、user-secrets）讀，正式環境沒設就不能啟動。
/// </summary>
public class JwtSettingsTests
{
    private static IConfiguration Config(string? key) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:Key"] = key, ["Jwt:Issuer"] = "LineConsole", ["Jwt:Audience"] = "LineConsoleUsers" })
            .Build();

    [Fact]
    public void 有設定金鑰_就用設定的()
    {
        var settings = JwtSettings.FromConfiguration(Config(new string('k', 40)), allowEphemeral: false);
        Assert.Equal(40, settings.Key.Length);
        Assert.False(settings.IsEphemeral);
    }

    [Fact]
    public void 正式環境沒設金鑰_不能啟動()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => JwtSettings.FromConfiguration(Config(null), allowEphemeral: false));
        Assert.Contains("Jwt__Key", ex.Message);
    }

    [Fact]
    public void 金鑰太短_不能啟動()
    {
        Assert.Throws<InvalidOperationException>(() => JwtSettings.FromConfiguration(Config("too-short"), allowEphemeral: false));
    }

    [Fact]
    public void 開發環境或展示模式沒設金鑰_自動產生暫時金鑰()
    {
        var a = JwtSettings.FromConfiguration(Config(null), allowEphemeral: true);
        var b = JwtSettings.FromConfiguration(Config(null), allowEphemeral: true);
        Assert.True(a.IsEphemeral);
        Assert.Equal(JwtSettings.MinKeyBytes, a.Key.Length);
        Assert.NotEqual(a.Key, b.Key);
    }

    [Fact]
    public void appsettings裡沒有金鑰()
    {
        var path = Path.Combine(RepoRoot.Path, "LineConsole.Server", "appsettings.json");
        var config = new ConfigurationBuilder().AddJsonFile(path).Build();
        Assert.True(string.IsNullOrEmpty(config["Jwt:Key"]), "appsettings.json 不可以放 Jwt:Key");
    }
}
