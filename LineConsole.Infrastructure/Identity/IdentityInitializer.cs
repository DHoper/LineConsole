using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace LineConsole.Infrastructure.Identity;

/// <summary>
/// 初始化角色與預設管理員帳號
/// </summary>
public static class IdentityInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // 建立 Admin 與 User 角色
        string[] roles = [RoleNames.Admin, RoleNames.User];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 建立預設管理員帳號
        var adminEmail = "admin@example.com";
        var adminPassword = "Admin123!"; 
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                AccountType = RoleNames.Admin
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
            }
            else
            {
                // 可記錄 log 或丟出例外
                throw new Exception("建立預設管理員帳號失敗: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
