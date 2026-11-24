using LineConsole.Application.Common.Interfaces;
using LineConsole.Application.Common.Models;
using LineConsole.Application.Infrastructure.Interfaces;
using LineConsole.Application.LineOfficialAccounts;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.LineOfficialAccounts.Models;
using LineConsole.Application.RichMenus;
using LineConsole.Application.RichMenus.Interfaces;
using LineConsole.Application.RichMenus.Models;
using LineConsole.Application.UserProfiles.Interfaces;
using LineConsole.Domain.Entities;
using LineConsole.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace LineConsole.Infrastructure.Demo;

/// <summary>
/// 建立展示資料。盡量走正式的流程，而不是直接寫資料表：
/// 展示帳號用 AccountManager 註冊，圖文選單用 RichMenuService 建立（會經過存取控制、LINE 的檢查、上傳圖片）。
/// 所以每次啟動展示模式，等於把「註冊 → 建立選單 → 設預設」完整跑一遍。
/// </summary>
public static class DemoSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await IdentityInitializer.SeedRolesAndAdminAsync(services);

        var first = DemoData.Accounts[0];
        var identityUserId = await services.GetRequiredService<IAccountManager>().RegisterAsync(new RegisterInput
        {
            Email = DemoData.Email,
            Password = DemoData.Password,
            DisplayName = DemoData.DisplayName,
            LineAccount = ToCreateModel(first)
        });

        // 第二個官方帳號：註冊流程只綁一個，其餘的直接加（M2 會有「新增官方帳號」的功能）
        var profile = await services.GetRequiredService<IUserProfileService>().GetByIdentityUserIdAsync(identityUserId)
                      ?? throw new InvalidOperationException("展示帳號的個人資料沒有建立成功");
        var accountRepo = services.GetRequiredService<ILineOfficialAccountRepository>();
        foreach (var account in DemoData.Accounts.Skip(1))
        {
            await accountRepo.AddAsync(LineOfficialAccount.Create(
                profile.Id, account.ChannelId, account.ChannelSecret, account.AccessToken, account.ChannelName), ct);
        }

        // 以展示帳號的身分建立圖文選單
        var richMenus = new RichMenuService(
            services.GetRequiredService<ILineClient>(),
            new OfficialAccountAccess(new SeedUser(identityUserId), accountRepo),
            services.GetRequiredService<IRichMenuRepository>());

        var accounts = await accountRepo.FindByUserProfileIdAsync(profile.Id, ct);
        foreach (var demo in DemoData.Accounts)
        {
            var accountId = accounts.Single(a => a.ChannelId == demo.ChannelId).Id;
            foreach (var menu in demo.Menus)
            {
                await using var image = OpenImage(menu.Image);
                var created = await richMenus.CreateRichMenuWithImageAsync(accountId, ToDto(menu), image, "image/jpeg", ct);
                if (menu.IsDefault)
                    await richMenus.SetDefaultRichMenuAsync(accountId, created.RichMenuId, ct);
            }
        }
    }

    private static LineOfficialAccountCreateModel ToCreateModel(DemoData.Account a) => new()
    {
        ChannelName = a.ChannelName,
        ChannelId = a.ChannelId,
        ChannelSecret = a.ChannelSecret,
        ChannelAccessToken = a.AccessToken
    };

    private static RichMenuDTO ToDto(DemoData.Menu m) => new()
    {
        Name = m.Name,
        ChatBarText = m.ChatBarText,
        Selected = m.IsDefault,
        Size = new MenuSize { Width = m.Width, Height = m.Height },
        Areas = m.Areas.Select(a => new MenuArea
        {
            Bounds = new MenuBounds { X = a.X, Y = a.Y, Width = a.Width, Height = a.Height },
            Action = new MenuAction { Type = a.Type, Text = a.Text, Uri = a.Uri }
        }).ToList()
    };

    private static Stream OpenImage(string name) =>
        typeof(DemoSeeder).Assembly.GetManifestResourceStream($"LineConsole.Infrastructure.Demo.Images.{name}.jpg")
        ?? throw new InvalidOperationException($"找不到展示圖片 {name}.jpg（內嵌資源）");

    private sealed class SeedUser(string identityUserId) : ICurrentUser
    {
        public string? IdentityUserId { get; } = identityUserId;
    }
}
