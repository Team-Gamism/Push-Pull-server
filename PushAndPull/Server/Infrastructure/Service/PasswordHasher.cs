using Server.Application.Port.Output;

namespace Server.Infrastructure.Service;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;
    
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}