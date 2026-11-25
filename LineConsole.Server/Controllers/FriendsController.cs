using LineConsole.Application.Friends;
using LineConsole.Server.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace LineConsole.Server.Controllers;

/// <summary>官方帳號的好友名單和儀表板（資料來自 Webhook 的 follow／unfollow／message 事件）</summary>
[ApiController]
[Route("api/accounts/{lineOfficialAccountId:guid}")]
public class FriendsController : ControllerBase
{
    private readonly IFriendService _friends;

    public FriendsController(IFriendService friends)
    {
        _friends = friends;
    }

    /// <param name="status">following／blocked，不填就是全部</param>
    /// <param name="keyword">用顯示名稱搜尋</param>
    [HttpGet("friends")]
    public async Task<ApiResponse<PagedResult<FriendItem>>> ListAsync([FromRoute] Guid lineOfficialAccountId,
        [FromQuery] string? status, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        ApiResponse<PagedResult<FriendItem>>.SuccessResponse(await _friends.ListAsync(lineOfficialAccountId, status, keyword, page, pageSize, ct));

    [HttpGet("dashboard")]
    public async Task<ApiResponse<DashboardResult>> DashboardAsync([FromRoute] Guid lineOfficialAccountId, CancellationToken ct) =>
        ApiResponse<DashboardResult>.SuccessResponse(await _friends.GetDashboardAsync(lineOfficialAccountId, ct));
}
