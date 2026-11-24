using Microsoft.Data.Sqlite;

namespace LineConsole.Infrastructure.Demo;

/// <summary>
/// 展示模式的 SQLite 記憶體資料庫。
/// 共享快取的記憶體資料庫在「最後一條連線關掉」時就消失，所以這裡一直開著一條連線，直到程式結束。
/// 每個 DbContext 還是各自開連線（連到同一個資料庫），不共用同一個連線物件，避免多執行緒問題。
/// 名稱每次都不同：同一個程序裡開好幾個主機（整合測試）也不會互相干擾。
/// </summary>
public sealed class DemoDatabase : IDisposable
{
    private readonly SqliteConnection _keepAlive;

    public DemoDatabase()
    {
        ConnectionString = $"Data Source=lineconsole-demo-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        _keepAlive = new SqliteConnection(ConnectionString);
        _keepAlive.Open();
    }

    public string ConnectionString { get; }

    public void Dispose() => _keepAlive.Dispose();
}
