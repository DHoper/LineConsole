using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Microsoft.AspNetCore.Authorization;

using LineConsole.Infrastructure.Data;
using LineConsole.Infrastructure.Data.Repositories;
using LineConsole.Infrastructure.Http;
using LineConsole.Infrastructure.Identity; 
using LineConsole.Infrastructure.Logging;
using LineConsole.Infrastructure.Security;
using LineConsole.Infrastructure.ExternalClients;
using LineConsole.Application.Common.Interfaces;
using LineConsole.Application.Infrastructure.Interfaces;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.RichMenus;
using LineConsole.Application.RichMenus.Interfaces;
using LineConsole.Application.UserProfiles;
using LineConsole.Application.UserProfiles.Interfaces;
using LineConsole.Server.Middlewares;
using LineConsole.Server.Security;
using LineConsole.Application.LineOfficialAccounts;

var builder = WebApplication.CreateBuilder(args);

#region Logging
builder.Host.UseSerilog((context, services, configuration) =>
{
    SerilogConfigurator.Configure(configuration);
});
#endregion

#region Services: MVC & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

#region EF Core - DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

#region Identity + JWT
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

// 簽發和驗證共用同一份設定；金鑰從環境變數 Jwt__Key 或 user-secrets 讀（不放 appsettings.json）
var jwtSettings = JwtSettings.FromConfiguration(builder.Configuration, builder.Environment);
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
        options.SaveToken = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
#endregion

#region DI 注入

// 目前的使用者（從 JWT 取）；官方帳號的存取控制
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<IOfficialAccountAccess, OfficialAccountAccess>();

// HTTP
builder.Services.AddScoped<IExternalHttpClient, ExternalHttpClient>();
builder.Services.AddHttpClient();

// LINE 模組
builder.Services.AddScoped<ILineClient, LineClient>();
builder.Services.AddScoped<IRichMenuService, RichMenuService>();
builder.Services.AddScoped<IRichMenuRepository, RichMenuRepository>();
builder.Services.AddScoped<ILineOfficialAccountRepository, LineOfficialAccountRepository>();

// 使用者模組（非 Identity）
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherAdapter>();

// 身分驗證（Identity + JWT）
builder.Services.AddScoped<IAccountManager, AccountManager>();
builder.Services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

#endregion

var app = builder.Build();

#region Middleware Pipeline

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseDefaultFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ApiExceptionMiddleware>();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("/index.html");

#endregion

if (jwtSettings.IsEphemeral)
    app.Logger.LogWarning("沒有設定 Jwt:Key，開發環境先用暫時金鑰：重開程式後要重新登入。");

#region 初始化角色（有設定 Seed:AdminEmail／AdminPassword 才建立管理員）

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentityInitializer.SeedRolesAndAdminAsync(services);
}

#endregion

app.Run();
