namespace Kernel.Domain;

public class ImageUrl : BaseValueObject
{
    public string Path { get; }

    private ImageUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new FormatException("Đường dẫn ảnh không được để trống.");

        // 1. Kiểm tra định dạng URI hợp lệ
        if (!Uri.TryCreate(path, UriKind.Absolute, out var uriResult) ||
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            throw new FormatException("Định dạng URL ảnh không hợp lệ.");
        }

        // 2. Tùy chọn: Kiểm tra phần mở rộng (extension)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
        var hasValidExtension = allowedExtensions.Any(ext =>
            path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));

        if (!hasValidExtension)
            throw new FormatException("Định dạng file ảnh không được hỗ trợ.");

        Path = path;
    }

    public static ImageUrl Create(string path) => new ImageUrl(path);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Path.ToLowerInvariant(); // So sánh không phân biệt hoa thường
    }

    public override string ToString() => Path;
}