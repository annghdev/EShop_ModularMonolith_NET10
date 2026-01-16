namespace Infrastructure;

public class CloudinarySettings
{
    public const string Position = "Cloudinary";
    public string CloudName { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
    public string ApiSecret { get; set; } = default!;
}
