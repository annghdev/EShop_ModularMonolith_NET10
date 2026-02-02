using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Kernel.Domain;

public class Slug : BaseValueObject
{
    public string Value { get; }

    private Slug() { }

    public Slug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new InputFormatException("Input slug không hợp lệ");

        // 1. Chuyển về chữ thường trước
        string str = input.ToLowerInvariant();

        // 2. Thay thế chữ đ/Đ trước khi xử lý dấu
        str = str.Replace('đ', 'd');

        // 3. Chuẩn hóa FormD và lọc dấu
        string normalizedString = str.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();

        foreach (var c in normalizedString)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        // 4. Normalize lại FormC
        string result = sb.ToString().Normalize(NormalizationForm.FormC);

        // 5. Regex: Thay thế ký tự đặc biệt thành dấu gạch ngang
        // Loại bỏ các ký tự không phải chữ cái/số
        result = Regex.Replace(result, @"[^a-z0-9\s-]", "");

        // 6. Xử lý khoảng trắng: Thay nhiều khoảng trắng/gạch ngang liên tiếp thành 1 dấu gạch ngang duy nhất
        result = Regex.Replace(result, @"[\s-]+", "-").Trim('-');

        Value = result;
    }
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
