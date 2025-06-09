using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace LineConsole.Infrastructure.Identity;

/// <summary>
/// ��l�ƨ���P�w�]�޲z���b��
/// </summary>
public static class IdentityInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // �إ� Admin �P User ����
        string[] roles = [RoleNames.Admin, RoleNames.User];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // �إ߹w�]�޲z���b���]�A�i�H�ۦ�վ� Email �P�K�X�^
        var adminEmail = "admin@example.com";
        var adminPassword = "Admin123!"; // �K�X�i�A�j��
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
                // �i�O�� log �Υ�X�ҥ~
                throw new Exception("�إ߹w�]�޲z���b������: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
