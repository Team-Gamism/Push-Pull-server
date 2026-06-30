using System.Net;
using Gamism.SDK.Extensions.AspNetCore.Exceptions;

namespace PushAndPull.Domain.Auth.Exception;

public class SteamApiException : ExpectedException
{
    public int? UpstreamStatusCode { get; }

    public SteamApiException(string message, int? upstreamStatusCode = null)
        : base(HttpStatusCode.ServiceUnavailable, message)
    {
        UpstreamStatusCode = upstreamStatusCode;
    }

    // ExpectedException carries no inner-exception constructor; the cause is identified
    // by the message and the original exception is dropped from the chain.
    public SteamApiException(string message, System.Exception innerException)
        : base(HttpStatusCode.ServiceUnavailable, message)
    {
    }
}
