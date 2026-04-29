using Gamism.SDK.Extensions.AspNetCore.Exceptions;

namespace PushAndPull.Domain.Auth.Exception;

public class InvalidNicknameException : BadRequestException
{
    public InvalidNicknameException()
        : base("INVALID_NICKNAME")
    {
    }
}
