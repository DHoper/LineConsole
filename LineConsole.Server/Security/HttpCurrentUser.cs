using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LineConsole.Application.Common.Interfaces;

namespace LineConsole.Server.Security;

/// <summary>
/// 從這次請求的 JWT 取出目前的使用者（sub 宣告＝Identity 使用者 ID）
/// </summary>
public class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string? IdentityUserId
    {
        get
        {
            var user = _accessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;
            // JwtBearer 預設會把 sub 對應成 NameIdentifier；兩個都看，不受對應設定影響
            return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
    }
}
