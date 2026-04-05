namespace PushAndPull.Domain.Auth.Service.Interface;

public interface ILogoutService
{
    Task ExecuteAsync(LogoutCommand request, CancellationToken ct = default);
}

public record LogoutCommand(string SessionId);
