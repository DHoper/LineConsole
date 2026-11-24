using LineConsole.Application;
using LineConsole.Application.Common.Interfaces;
using LineConsole.Infrastructure;
using LineConsole.Infrastructure.Identity;
using LineConsole.Infrastructure.Logging;
using LineConsole.Server.Middlewares;
using LineConsole.Server.Models.Api;
using LineConsole.Server.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => SerilogConfigurator.Configure(configuration));

// 執行模式：Demo（預設：SQLite 記憶體資料庫＋模擬 LINE＋展示資料）或 Live（SQL Server＋真的 LINE）
var mode = builder.Configuration.GetAppMode();

#region 服務
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, mode);

// 目前的使用者（從 JWT 取，給應用層做存取控制）
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

// JWT：簽發和驗證共用同一份設定。金鑰從環境變數 Jwt__Key 或 user-secrets 讀（不放 appsettings.json）；
// 展示模式、開發環境沒設時自動產生暫時金鑰
var jwtSettings = JwtSettings.FromConfiguration(
    builder.Configuration, allowEphemeral: builder.Environment.IsDevelopment() || mode == AppMode.Demo);
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtSettings.Key)
        };
    });

// 預設所有 API 都要登入；不用登入的（登入、註冊、系統資訊、前端頁面）個別標 AllowAnonymous
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});
#endregion

var app = builder.Build();

app.Logger.LogInformation("LineConsole 以 {Mode} 模式啟動", mode);
if (jwtSettings.IsEphemeral)
    app.Logger.LogWarning("沒有設定 Jwt:Key，先用暫時金鑰：重開程式後要重新登入。");

await app.Services.InitializeDatabaseAsync();

#region 中介層
app.UseSerilogRequestLogging();

if (mode == AppMode.Live)
    app.UseHttpsRedirection();

// 前端（Vite 建置到 wwwroot）
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment() || mode == AppMode.Demo)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 沒有對應的 /api 路徑回 404 JSON，不要回前端頁面
app.MapFallback("/api/{**path}", () =>
        Results.Json(ApiResponse<ApiEmptyResult>.FailResponse("NOT_FOUND", "找不到這個 API"), statusCode: StatusCodes.Status404NotFound))
    .AllowAnonymous();

// 其他路徑都交給前端路由（React Router）
app.MapFallbackToFile("index.html").AllowAnonymous();
#endregion

app.Run();

/// <summary>給整合測試（WebApplicationFactory）用</summary>
public partial class Program;
