using LineConsole.Application.Common.Interfaces;
using LineConsole.Application.Infrastructure.Interfaces;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.RichMenus.Interfaces;
using LineConsole.Application.UserProfiles.Interfaces;
using LineConsole.Infrastructure.Data;
using LineConsole.Infrastructure.Data.Repositories;
using LineConsole.Infrastructure.Demo;
using LineConsole.Infrastructure.Demo.FakeLine;
using LineConsole.Infrastructure.ExternalClients;
using LineConsole.Infrastructure.Http;
using LineConsole.Infrastructure.Identity;
using LineConsole.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LineConsole.Infrastructure;

/// <summary>
/// 基礎設施層的服務註冊。展示／正式模式的差別都集中在這裡：
/// 只換掉「資料庫」和「LINE 客戶端」兩個實作，應用層和 API 完全一樣。
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, AppMode mode)
    {
        services.AddSingleton(new RuntimeInfo(mode));

        if (mode == AppMode.Demo)
        {
            var demoDb = new DemoDatabase();
            services.AddSingleton(demoDb);
            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(demoDb.ConnectionString));
            services.AddSingleton<FakeLineStore>();
            services.AddScoped<ILineClient, FakeLineClient>();
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Live 模式需要 ConnectionStrings:DefaultConnection（SQL Server）。");

            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connectionString));
            services.AddHttpClient();
            services.AddScoped<IExternalHttpClient, ExternalHttpClient>();
            services.AddScoped<ILineClient, LineClient>();
        }

        // 身分驗證（ASP.NET Identity，資料存在同一個資料庫）
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IAccountManager, AccountManager>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasherAdapter>();

        // 資料存取
        services.AddScoped<ILineOfficialAccountRepository, LineOfficialAccountRepository>();
        services.AddScoped<IRichMenuRepository, RichMenuRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        return services;
    }

    /// <summary>
    /// 啟動時準備資料庫。展示模式：建立記憶體資料庫的資料表並放入展示資料。正式模式：套用 migration、建立角色。
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var db = provider.GetRequiredService<ApplicationDbContext>();

        if (provider.GetRequiredService<RuntimeInfo>().Mode == AppMode.Demo)
        {
            await db.Database.EnsureCreatedAsync(ct);
            await DemoSeeder.SeedAsync(provider, ct);
        }
        else
        {
            await db.Database.MigrateAsync(ct);
            await IdentityInitializer.SeedRolesAndAdminAsync(provider);
        }
    }
}
