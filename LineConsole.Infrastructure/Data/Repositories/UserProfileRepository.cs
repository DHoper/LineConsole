using LineConsole.Application.Users.Interfaces;
using LineConsole.Domain.Entities;
using LineConsole.Infrastructure.Data;
using LineConsole.Infrastructure.Data.EfEntities;
using Microsoft.EntityFrameworkCore;

namespace LineConsole.Infrastructure.Data.Repositories;

/// <summary>
/// 使用者擴充資料存取實作，透過 EF Core 操作 user_profiles 資料表
/// </summary>
public class UserProfileRepository : IUserProfileRepository
{
    private readonly ApplicationDbContext _db;

    public UserProfileRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 新增一筆 UserProfile 資料
    /// </summary>
    public async Task AddAsync(UserProfile user)
    {
        var entity = new UserProfileEntity
        {
            Id = user.Id,
            IdentityUserId = user.IdentityUserId,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            OrganizationCode = user.OrganizationCode,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        _db.UserProfiles.Add(entity);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// 根據 IdentityUserId 查詢 UserProfile
    /// </summary>
    public async Task<UserProfile?> FindByIdentityUserIdAsync(string identityUserId)
    {
        var entity = await _db.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdentityUserId == identityUserId);

        if (entity is null) return null;

        return new UserProfile
        {
            Id = entity.Id,
            IdentityUserId = entity.IdentityUserId,
            DisplayName = entity.DisplayName,
            AvatarUrl = entity.AvatarUrl,
            OrganizationCode = entity.OrganizationCode,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    /// <summary>
    /// 刪除指定 UserProfile（依 Guid）
    /// </summary>
    public async Task DeleteAsync(Guid userProfileId)
    {
        var entity = await _db.UserProfiles.FindAsync(userProfileId);
        if (entity is null) return;

        _db.UserProfiles.Remove(entity);
        await _db.SaveChangesAsync();
    }
}
