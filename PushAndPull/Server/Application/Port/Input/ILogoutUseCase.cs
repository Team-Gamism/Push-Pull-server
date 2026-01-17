namespace Server.Application.Port.Input;

public interface ILogoutUseCase
{
    Task ExecuteAsync(LogoutCommand request);
}

public record LogoutCommand(string SessionId);