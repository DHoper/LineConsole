using LineConsole.Application.Common.Interfaces;
using LineConsole.Application.Common.Models;
using LineConsole.Application.LineOfficialAccounts;
using LineConsole.Domain.Entities;
using LineConsole.Infrastructure.Data;
using LineConsole.Infrastructure.Data.EfEntities;
using LineConsole.Infrastructure.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LineConsole.Tests.Security;

/// <summary>
/// 越權修正：使用者只能拿到「自己的」官方帳號的 Access Token。
/// 用真的 EF Core 查詢（SQLite 記憶體資料庫），確認過濾條件真的落在資料庫查詢上。
/// </summary>
public sealed class OfficialAccountAccessTests : IDisposable
{
    private const string AliceId = "identity-alice";
    private const string BobId = "identity-bob";

    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _db;
    private readonly Guid _aliceAccountId;

    public OfficialAccountAccessTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();

        var alice = AddProfile(AliceId);
        AddProfile(BobId);
        _aliceAccountId = Guid.NewGuid();
        _db.LineOfficialAccounts.Add(new LineOfficialAccountEntity
        {
            Id = _aliceAccountId,
            UserProfileId = alice,
            ChannelId = "1234567890",
            ChannelName = "Alice 的咖啡店",
            ChannelSecret = "alice-secret",
            ChannelAccessToken = "alice-token",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        _db.SaveChanges();
    }

    private Guid AddProfile(string identityUserId)
    {
        var profile = UserProfile.Create(identityUserId, identityUserId);
        _db.UserProfiles.Add(new UserProfileEntity
        {
            Id = profile.Id,
            IdentityUserId = identityUserId,
            DisplayName = identityUserId,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        });
        return profile.Id;
    }

    private OfficialAccountAccess AccessAs(string? identityUserId) =>
        new(new FixedUser(identityUserId), new LineOfficialAccountRepository(_db));

    [Fact]
    public async Task 擁有者拿得到自己的Token()
    {
        var token = await AccessAs(AliceId).GetAccessTokenAsync(_aliceAccountId, TestContext.Current.CancellationToken);
        Assert.Equal("alice-token", token);
    }

    [Fact]
    public async Task 別人帶我的帳號ID_回找不到()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => AccessAs(BobId).GetAccessTokenAsync(_aliceAccountId, TestContext.Current.CancellationToken));
        Assert.Equal(OfficialAccountAccess.NotFoundCode, ex.Code);
    }

    [Fact]
    public async Task 沒登入_回找不到()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => AccessAs(null).GetAccessTokenAsync(_aliceAccountId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task 不存在的帳號_跟別人的帳號回一樣的錯誤()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => AccessAs(AliceId).GetAccessTokenAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));
        Assert.Equal(OfficialAccountAccess.NotFoundCode, ex.Code);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private sealed class FixedUser(string? id) : ICurrentUser
    {
        public string? IdentityUserId { get; } = id;
    }
}
