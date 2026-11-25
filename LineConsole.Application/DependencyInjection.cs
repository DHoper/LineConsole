using LineConsole.Application.Friends;
using LineConsole.Application.LineOfficialAccounts;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.RichMenus;
using LineConsole.Application.RichMenus.Interfaces;
using LineConsole.Application.UserProfiles;
using LineConsole.Application.UserProfiles.Interfaces;
using LineConsole.Application.Webhooks;
using Microsoft.Extensions.DependencyInjection;

namespace LineConsole.Application;

/// <summary>應用層的服務註冊（用例、存取控制）。實作細節在 Infrastructure，這裡只依賴介面。</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IOfficialAccountAccess, OfficialAccountAccess>();
        services.AddScoped<IOfficialAccountService, OfficialAccountService>();
        services.AddScoped<IRichMenuService, RichMenuService>();
        services.AddScoped<IRichMenuScheduleService, RichMenuScheduleService>();
        services.AddScoped<IRichMenuScheduleRunner, RichMenuScheduleRunner>();
        services.AddScoped<ILineWebhookService, LineWebhookService>();
        services.AddScoped<IWebhookEventProcessor, WebhookEventProcessor>();
        services.AddScoped<IFriendService, FriendService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        return services;
    }
}
