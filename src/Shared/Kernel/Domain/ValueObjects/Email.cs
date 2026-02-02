using System.Text.RegularExpressions;

namespace Kernel.Domain;

public class Email : BaseValueObject
{
    // Regex cơ bản để kiểm tra định dạng email
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email() { }

    public Email(string value)
    {
        // 1. Validation logic ngay tại constructor
        if (string.IsNullOrWhiteSpace(value))
            throw new InputFormatException("Email không được để trống.");

        if (value.Length > 255)
            throw new InputFormatException("Email quá dài.");

        if (!EmailRegex.IsMatch(value))
            throw new InputFormatException("Định dạng Email không hợp lệ.");

        Value = value.ToLowerInvariant(); // Chuẩn hóa dữ liệu
    }

    // 2. Override GetEqualityComponents để so sánh bằng giá trị
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    // Helper để in ra string trực tiếp
    public override string ToString() => Value;
}