namespace Server.Application.Port.Input;

public interface ILoginUseCase
{
    Task<LoginResult> ExecuteAsync(LoginCommand request);
}

public record LoginCommand(string Ticket, string Nickname);
public record LoginResult(string SessionId);