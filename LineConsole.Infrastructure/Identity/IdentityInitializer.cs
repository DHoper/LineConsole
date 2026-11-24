using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LineConsole.Infrastructure.Identity;

/// <summary>
/// 初始化角色；有設定 Seed:AdminEmail、Seed:AdminPassword 時才建立管理員帳號（帳密不寫在程式裡）
/// </summary>
public static class IdentityInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        foreach (var role in new[] { RoleNames.Admin, RoleNames.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminEmail = configuration["Seed:AdminEmail"];
        var adminPassword = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            AccountType = RoleNames.Admin
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException("建立管理員帳號失敗：" + string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
    }
}
