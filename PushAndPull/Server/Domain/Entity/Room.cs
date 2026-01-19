namespace Server.Domain.Entity;

public class Room
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public bool IsPrivate { get; set; }
}