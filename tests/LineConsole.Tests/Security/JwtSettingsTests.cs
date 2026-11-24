using LineConsole.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

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
        var settings = JwtSettings.FromConfiguration(Config(new string('k', 40)), new FakeEnv(Environments.Production));
        Assert.Equal(40, settings.Key.Length);
        Assert.False(settings.IsEphemeral);
    }

    [Fact]
    public void 正式環境沒設金鑰_不能啟動()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => JwtSettings.FromConfiguration(Config(null), new FakeEnv(Environments.Production)));
        Assert.Contains("Jwt__Key", ex.Message);
    }

    [Fact]
    public void 金鑰太短_不能啟動()
    {
        Assert.Throws<InvalidOperationException>(() => JwtSettings.FromConfiguration(Config("too-short"), new FakeEnv(Environments.Production)));
    }

    [Fact]
    public void 開發環境沒設金鑰_自動產生暫時金鑰()
    {
        var a = JwtSettings.FromConfiguration(Config(null), new FakeEnv(Environments.Development));
        var b = JwtSettings.FromConfiguration(Config(null), new FakeEnv(Environments.Development));
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

    private sealed class FakeEnv(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "LineConsole.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
