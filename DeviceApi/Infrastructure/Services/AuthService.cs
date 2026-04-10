using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DeviceApi.Application.Contracts;
using DeviceApi.Application.Interfaces;
using DeviceApi.Infrastructure.Identity;
using DeviceApi.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DeviceApi.Infrastructure.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    IOperationLogService operationLogService,
    IOptions<JwtOptions> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<(bool Succeeded, IReadOnlyCollection<string> Errors)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = new AppUser
        {
            UserName = request.UserName.Trim(),
            Email = $"{request.UserName.Trim()}@device.local"
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(x => x.Description).ToArray());
        }

        var role = string.IsNullOrWhiteSpace(request.Role) ? AppRoles.Viewer : request.Role.Trim();
        if (role is not (AppRoles.Admin or AppRoles.Operator or AppRoles.Viewer))
        {
            role = AppRoles.Viewer;
        }

        await userManager.AddToRoleAsync(user, role);
        await operationLogService.WriteAsync("RegisterUser", "User", user.Id, $"注册用户:{user.UserName},角色:{role}", cancellationToken);
        logger.LogInformation("用户注册成功: {UserName}", user.UserName);
        return (true, Array.Empty<string>());
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(request.UserName.Trim());
        if (user is null)
        {
            return null;
        }

        var passwordCorrect = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordCorrect)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.ExpireMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        await operationLogService.WriteAsync("Login", "User", user.Id, $"用户登录:{user.UserName}", cancellationToken);
        logger.LogInformation("用户登录成功: {UserName}", user.UserName);
        return new AuthResponse(tokenString, expiresAt, user.UserName ?? request.UserName.Trim(), roles.ToArray());
    }
}
