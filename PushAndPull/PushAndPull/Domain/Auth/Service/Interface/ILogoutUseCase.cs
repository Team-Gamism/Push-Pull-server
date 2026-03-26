namespace PushAndPull.Domain.Auth.Service.Interface;

public interface ILogoutUseCase
{
    Task ExecuteAsync(LogoutCommand request);
}

public record LogoutCommand(string SessionId);
