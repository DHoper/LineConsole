using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LineConsole.Infrastructure.Data;

/// <summary>
/// 給 dotnet ef（migrations add、has-pending-model-changes）用的 DbContext：一律用 SQL Server。
/// 程式預設是展示模式（SQLite），如果讓 EF 工具照程式的設定建 DbContext，產生的 migration 會變成 SQLite 的型別。
/// migration 只給正式模式（SQL Server）用；展示模式用 EnsureCreated 直接照模型建表。
/// 產生 migration 不會真的連線，連線字串只是佔位；要套用到資料庫時用 ConnectionStrings__DefaultConnection 指定。
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                               ?? "Server=(localdb)\\MSSQLLocalDB;Database=LineConsole;Trusted_Connection=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new ApplicationDbContext(options);
    }
}
