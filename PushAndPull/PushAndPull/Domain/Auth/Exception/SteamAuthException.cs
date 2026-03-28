using Gamism.SDK.Extensions.AspNetCore.Exceptions;
using System.Net;

namespace PushAndPull.Domain.Auth.Exception;

public abstract class SteamAuthException : ExpectedException
{
    public ulong SteamId { get; }

    protected SteamAuthException(HttpStatusCode statusCode, string message, ulong steamId = 0)
        : base(statusCode, message)
    {
        SteamId = steamId;
    }
}
