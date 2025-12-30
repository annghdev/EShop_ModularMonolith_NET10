namespace Kernel.Domain;

public abstract class BaseValueObject
{
    /// <summary>
    /// Các thành phần để so sánh equality của VO.
    /// VO con phải override cái này và yield return các field/property cần so sánh.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (BaseValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        // bắt đầu với số nguyên tố 17, nhân với 23 mỗi vòng để giảm collision
        return GetEqualityComponents()
            .Aggregate(17, (current, obj) =>
            {
                unchecked
                {
                    return current * 23 + (obj?.GetHashCode() ?? 0);
                }
            });
    }

    public static bool operator ==(BaseValueObject? left, BaseValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(BaseValueObject? left, BaseValueObject? right)
    {
        return !(left == right);
    }

}
