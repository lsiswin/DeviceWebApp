using DeviceApi.Application.Contracts;

namespace DeviceApi.Application.Interfaces;

public interface IAuthService
{
    Task<(bool Succeeded, IReadOnlyCollection<string> Errors)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
