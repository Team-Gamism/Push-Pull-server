using System.Security.Cryptography;
using Server.Application.Port.Output;

namespace Server.Infrastructure.Service;

public class RoomCodeGenerator : IRoomCodeGenerator
{
    private readonly string _allowedChars;
    private readonly int _codeLength;

    public RoomCodeGenerator(
        string allowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789",
        int codeLength = 6
    )
    {
        _allowedChars = allowedChars;
        _codeLength = codeLength;
    }

    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(_codeLength);
        var chars = new char[_codeLength];

        for (int i = 0; i < _codeLength; i++)
        {
            chars[i] = _allowedChars[bytes[i] % _allowedChars.Length];
        }

        return new string(chars);
    }
}
