using System.Text;
using DeviceApi.Application.Contracts;
using DeviceApi.Application.Interfaces;
using DeviceApi.Infrastructure.Data;
using DeviceApi.Infrastructure.Identity;
using DeviceApi.Infrastructure.Options;
using DeviceApi.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    }
);

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AdminWeb",
        cors =>
        {
            cors.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    );
});
builder
    .Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
    });
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var connectionString =
    builder.Configuration.GetConnectionString("SqlServer")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=DeviceWebAppDb;Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder
    .Services.AddIdentityCore<AppUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

var jwtSection =
    builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection.Key));
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection.Issuer,
            ValidAudience = jwtSection.Audience,
            IssuerSigningKey = jwtKey,
            ClockSkew = TimeSpan.FromMinutes(2),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AppPolicies.DeviceRead,
        policy => policy.RequireRole(AppRoles.Admin, AppRoles.Operator, AppRoles.Viewer)
    );
    options.AddPolicy(
        AppPolicies.DeviceWrite,
        policy => policy.RequireRole(AppRoles.Admin, AppRoles.Operator)
    );
    options.AddPolicy(
        AppPolicies.DashboardRead,
        policy => policy.RequireRole(AppRoles.Admin, AppRoles.Operator, AppRoles.Viewer)
    );
    options.AddPolicy(
        AppPolicies.AuditRead,
        policy => policy.RequireRole(AppRoles.Admin, AppRoles.Operator)
    );
    options.AddPolicy(AppPolicies.UserManage, policy => policy.RequireRole(AppRoles.Admin));
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IDataPointService, DataPointService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOperationLogService, OperationLogService>();
builder.Services.AddScoped<IPlcDataService, PlcDataService>();
builder.Services.AddScoped<IAlarmService, AlarmService>();

var app = builder.Build();

// ====== 插入种子数据初始化逻辑 ======
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 确保数据库已经创建并应用了所有最新的迁移
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        // 执行账号初始化
        await IdentityDataSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "初始化数据库种子数据时发生错误。");
    }
}

// =====================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AdminWeb");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
