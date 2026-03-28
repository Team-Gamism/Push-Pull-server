namespace PushAndPull.Domain.Auth.Service.Interface;

public interface ILoginService
{
    Task<LoginResult> ExecuteAsync(LoginCommand request);
}

public record LoginCommand(string Ticket, string Nickname);
public record LoginResult(string SessionId);
