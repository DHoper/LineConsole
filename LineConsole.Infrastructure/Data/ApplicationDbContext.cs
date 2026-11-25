using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LineConsole.Infrastructure.Data.EfEntities;
using LineConsole.Infrastructure.Identity;

namespace LineConsole.Infrastructure.Data;

/// <summary>
/// EF Core 資料庫上下文，負責管理資料表對應與連線。
/// 也存 ASP.NET Data Protection 的金鑰（IDataProtectionKeyContext）：加密官方帳號憑證用的金鑰跟資料放在一起，
/// 容器重開、換機器都還解得開。
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
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

    /// <summary>ASP.NET Data Protection 的金鑰</summary>
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 用 LINE 的選單 ID 找本地紀錄（刪除、排程都會用到）
        builder.Entity<RichMenuEntity>()
            .HasIndex(x => new { x.LineOfficialAccountId, x.LineRichMenuId });

        // 背景服務每次都要找「該開始／該結束」的排程
        builder.Entity<RichMenuScheduleEntity>()
            .HasIndex(x => new { x.Status, x.StartTime });
        builder.Entity<RichMenuScheduleEntity>()
            .HasIndex(x => x.AccountId);
    }
}
