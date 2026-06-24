namespace PushAndPull.Domain.Auth.Exception;

public class InvalidTicketException : SteamAuthException
{
    public InvalidTicketException(string message = "Invalid authentication ticket")
        : base(message)
    {
    }
}
