using System.Security.Cryptography;
using Server.Application.Port.Output;

namespace Server.Infrastructure.Service;

public class RoomCodeGenerator : IRoomCodeGenerator
{
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 6;
    
    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(CodeLength);
        var chars = new char[CodeLength];

        for (int i = 0; i < CodeLength; i++)
        {
            chars[i] = AllowedChars[bytes[i] % AllowedChars.Length];
        }

        return new string(chars);
    }
}