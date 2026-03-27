namespace PushAndPull.Domain.Auth.Service.Interface;

public interface ILogoutService
{
    Task ExecuteAsync(LogoutCommand request);
}

public record LogoutCommand(string SessionId);
