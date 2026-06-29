namespace PushAndPull.Domain.Auth.Exception;

public class SteamApiException : System.Exception
{
    public int? StatusCode { get; }

    public SteamApiException(string message, int? statusCode = null)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public SteamApiException(string message, System.Exception innerException)
        : base(message, innerException)
    {
    }
}
