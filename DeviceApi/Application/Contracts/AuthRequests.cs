namespace DeviceApi.Application.Contracts;

public sealed record RegisterRequest(string UserName, string Password, string Role);

public sealed record LoginRequest(string UserName, string Password);

public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, string UserName, IReadOnlyCollection<string> Roles);
