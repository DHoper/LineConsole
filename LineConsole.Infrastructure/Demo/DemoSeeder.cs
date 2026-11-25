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
using LineConsole.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace LineConsole.Infrastructure.Demo;

/// <summary>
/// 建立展示資料。全部走正式的流程，而不是直接寫資料表：
/// 註冊（AccountManager）→ 新增官方帳號（OfficialAccountService，會向 LINE 驗證 token）
/// → 建立圖文選單（RichMenuService）→ 設預設 → 建立排程（RichMenuScheduleService）。
/// 所以每次啟動展示模式，等於把主要流程完整跑一遍。
/// </summary>
public static class DemoSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await IdentityInitializer.SeedRolesAndAdminAsync(services);

        var identityUserId = await services.GetRequiredService<IAccountManager>().RegisterAsync(new RegisterInput
        {
            Email = DemoData.Email,
            Password = DemoData.Password,
            DisplayName = DemoData.DisplayName
        });

        // 以展示帳號的身分操作
        var me = new SeedUser(identityUserId);
        var line = services.GetRequiredService<ILineClient>();
        var accountRepo = services.GetRequiredService<ILineOfficialAccountRepository>();
        var access = new OfficialAccountAccess(me, accountRepo);
        var accounts = new OfficialAccountService(me, services.GetRequiredService<IUserProfileRepository>(), accountRepo, line);
        var menuRepo = services.GetRequiredService<IRichMenuRepository>();
        var richMenus = new RichMenuService(line, access, menuRepo);
        var time = services.GetRequiredService<TimeProvider>();
        var schedules = new RichMenuScheduleService(access, menuRepo, services.GetRequiredService<IRichMenuScheduleRepository>(), time);
        var now = time.GetUtcNow();

        foreach (var demo in DemoData.Accounts)
        {
            var account = await accounts.CreateAsync(new CreateOfficialAccountInput
            {
                ChannelName = demo.ChannelName,
                ChannelId = demo.ChannelId,
                ChannelSecret = demo.ChannelSecret,
                ChannelAccessToken = demo.AccessToken
            }, ct);

            foreach (var menu in demo.Menus)
            {
                await using var image = OpenImage(menu.Image);
                var created = await richMenus.CreateRichMenuWithImageAsync(account.Id, ToDto(menu), image, "image/jpeg", ct);
                if (menu.IsDefault)
                    await richMenus.SetDefaultRichMenuAsync(account.Id, created.RichMenuId, ct);

                if (menu.Schedule is { } s)
                {
                    var start = now.AddMinutes(s.StartAfterMinutes);
                    await schedules.CreateAsync(account.Id, new CreateRichMenuScheduleInput
                    {
                        RichMenuId = created.RichMenuId,
                        StartTime = start,
                        EndTime = start.AddMinutes(s.DurationMinutes)
                    }, ct);
                }
            }
        }
    }

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
