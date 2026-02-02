namespace Kernel.Application;

public class FileMetadata
{
    public int Id { get; set; }
    public string Resource { get; set; } = default!; // e.g., "Product", "User"
    public string EntityId { get; set; } = default!;
    public string Url { get; set; } = default!;
    public string ExternalId { get; set; } = default!;
    public string Folder { get; set; } = default!;
    public string Type { get; set; } = "Image";
    public string Format { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}
