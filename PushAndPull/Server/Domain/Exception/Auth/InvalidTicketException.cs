namespace Server.Domain.Exception.Auth;

public class InvalidTicketException : SteamAuthException
{
    public InvalidTicketException(string message = "Invalid authentication ticket") 
        : base(message)
    {
    }
}