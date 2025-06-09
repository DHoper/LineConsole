using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LineConsole.Infrastructure.Data.EfEntities;
using LineConsole.Infrastructure.Identity;

namespace LineConsole.Infrastructure.Data;

/// <summary>
/// EF Core 資料庫上下文，負責管理資料表對應與連線
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>使用者擴充資料（對應 ASP.NET Identity 帳號）</summary>
    public DbSet<UserProfileEntity> UserProfiles { get; set; } = null!;

    /// <summary>LINE 官方帳號</summary>
    public DbSet<LineOfficialAccountEntity> LineOfficialAccounts { get; set; } = null!;

    /// <summary>Rich Menu 主資料表</summary>
    public DbSet<RichMenuEntity> RichMenus { get; set; } = null!;

    /// <summary>Rich Menu 點擊區域</summary>
    public DbSet<RichMenuAreaEntity> RichMenuAreas { get; set; } = null!;

    /// <summary>Rich Menu 排程資料</summary>
    public DbSet<RichMenuScheduleEntity> RichMenuSchedules { get; set; } = null!;
}
