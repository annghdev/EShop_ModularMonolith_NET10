namespace Infrastructure;

public class EmailSettings
{
    public const string Position = "EmailSettings";

    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string From { get; set; } = default!;
}
