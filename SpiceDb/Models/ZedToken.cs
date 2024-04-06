namespace SpiceDb.Models;

public class ZedToken
{
    public ZedToken(string? token)
    {
        Token = token ?? string.Empty;
    }

    public string Token { get; set; }
}
