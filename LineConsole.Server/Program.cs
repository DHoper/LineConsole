using System.Text;
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
using LineConsole.Application.Users;
using LineConsole.Application.Users.Interfaces;
using LineConsole.Server.Middlewares;

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
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
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

#region CORS（如需前後端分離）
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});
#endregion

#region DI 注入

// HTTP
builder.Services.AddScoped<IExternalHttpClient, ExternalHttpClient>();
builder.Services.AddHttpClient();

// LINE 模組
builder.Services.AddScoped<ILineClient, LineClient>();
builder.Services.AddScoped<IRichMenuService, RichMenuService>();
builder.Services.AddScoped<IRichMenuRepository, RichMenuRepository>();
builder.Services.AddScoped<ILineOfficialAccountRepository, LineOfficialAccountRepository>();

// 使用者模組（非 Identity）
builder.Services.AddScoped<IUserProfileService, UserService>();
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

app.UseCors();
app.UseMiddleware<ApiExceptionMiddleware>();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("/index.html");

#endregion

#region 初始化角色與預設管理員帳號 ✅

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentityInitializer.SeedRolesAndAdminAsync(services);
}

#endregion

app.Run();
