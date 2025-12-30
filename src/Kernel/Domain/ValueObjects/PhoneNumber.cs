using System.Text.RegularExpressions;

namespace Kernel.Domain;

public class PhoneNumber : BaseValueObject
{
    // Regex đơn giản cho số điện thoại (VD: 9-11 chữ số)
    private static readonly Regex PhoneRegex = new(
        @"^\+?[0-9]{9,12}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new FormatException("Số điện thoại không được để trống.");

        // Chuẩn hóa: Loại bỏ khoảng trắng, dấu chấm, dấu gạch ngang
        var normalizedValue = value.Replace(" ", "").Replace("-", "").Replace(".", "");

        if (!PhoneRegex.IsMatch(normalizedValue))
            throw new FormatException("Định dạng số điện thoại không hợp lệ.");

        Value = normalizedValue;
    }

    public static PhoneNumber Create(string value)
    {
        return new PhoneNumber(value);
    }

    // So sánh dựa trên giá trị chuỗi đã chuẩn hóa
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}