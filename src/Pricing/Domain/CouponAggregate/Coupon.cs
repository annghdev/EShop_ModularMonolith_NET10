namespace Pricing.Domain;

public class Coupon : AggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DiscountValue Discount { get; private set; }
    public Money? MinOrderValue { get; private set; }
    public Money? MaxDiscountAmount { get; private set; }
    public int? UsageLimitTotal { get; private set; }
    public int? UsageLimitPerCustomer { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public CouponStatus Status { get; private set; }

    private readonly List<CouponCondition> _conditions = [];
    public IReadOnlyCollection<CouponCondition> Conditions => _conditions.AsReadOnly();

    private readonly List<CouponUsage> _usages = [];
    public IReadOnlyCollection<CouponUsage> Usages => _usages.AsReadOnly();

    public int UsedCount => _usages.Count;

    private Coupon() { } // EF Core

    public static Coupon Create(
        string code,
        string name,
        string? description,
        DiscountValue discount,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        Money? minOrderValue = null,
        Money? maxDiscountAmount = null,
        int? usageLimitTotal = null,
        int? usageLimitPerCustomer = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Coupon code is required");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Coupon name is required");

        if (endDate <= startDate)
            throw new DomainException("End date must be after start date");

        return new Coupon
        {
            Code = code.ToUpperInvariant(),
            Name = name,
            Description = description,
            Discount = discount ?? throw new DomainException("Discount is required"),
            StartDate = startDate,
            EndDate = endDate,
            MinOrderValue = minOrderValue,
            MaxDiscountAmount = maxDiscountAmount,
            UsageLimitTotal = usageLimitTotal,
            UsageLimitPerCustomer = usageLimitPerCustomer,
            Status = CouponStatus.Draft
        };
    }

    public void Activate()
    {
        if (Status == CouponStatus.Expired)
            throw new DomainException("Cannot activate an expired coupon");

        Status = CouponStatus.Active;
        AddEvent(new CouponActivatedEvent(Id, Code));
        IncreaseVersion();
    }

    public void Deactivate()
    {
        Status = CouponStatus.Disabled;
        AddEvent(new CouponDeactivatedEvent(Id, Code));
        IncreaseVersion();
    }

    public void AddCondition(CouponConditionType type, Guid? targetId = null, int? minQuantity = null)
    {
        var condition = new CouponCondition(type, targetId, minQuantity);
        _conditions.Add(condition);
        IncreaseVersion();
    }

    public void RemoveCondition(Guid conditionId)
    {
        var condition = _conditions.FirstOrDefault(c => c.Id == conditionId);
        if (condition != null)
        {
            _conditions.Remove(condition);
            IncreaseVersion();
        }
    }

    public bool CanApply(Guid customerId, Money orderTotal, DateTimeOffset? currentTime = null)
    {
        var now = currentTime ?? DateTimeOffset.UtcNow;

        // Check status
        if (Status != CouponStatus.Active)
            return false;

        // Check date range
        if (now < StartDate || now > EndDate)
            return false;

        // Check minimum order value
        if (MinOrderValue != null && orderTotal.Amount < MinOrderValue.Amount)
            return false;

        // Check total usage limit
        if (UsageLimitTotal.HasValue && UsedCount >= UsageLimitTotal.Value)
            return false;

        // Check per-customer usage limit
        if (UsageLimitPerCustomer.HasValue)
        {
            var customerUsageCount = _usages.Count(u => u.CustomerId == customerId);
            if (customerUsageCount >= UsageLimitPerCustomer.Value)
                return false;
        }

        return true;
    }

    public Money CalculateDiscount(Money orderTotal)
    {
        var discountAmount = Discount.CalculateDiscount(orderTotal);

        // Apply max discount limit for percentage discounts
        if (MaxDiscountAmount != null && discountAmount.Amount > MaxDiscountAmount.Amount)
        {
            return MaxDiscountAmount;
        }

        return discountAmount;
    }

    public void Use(Guid customerId, Guid orderId, Money discountApplied)
    {
        if (!CanApply(customerId, discountApplied))
            throw new DomainException("Coupon cannot be applied");

        var usage = new CouponUsage(Id, customerId, orderId, discountApplied);
        _usages.Add(usage);

        AddEvent(new CouponUsedEvent(Id, Code, customerId, orderId, discountApplied));

        // Check if exhausted
        if (UsageLimitTotal.HasValue && UsedCount >= UsageLimitTotal.Value)
        {
            AddEvent(new CouponExhaustedEvent(Id, Code));
        }

        IncreaseVersion();
    }

    public void UpdateDetails(
        string name,
        string? description,
        DiscountValue discount,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        Money? minOrderValue,
        Money? maxDiscountAmount,
        int? usageLimitTotal,
        int? usageLimitPerCustomer)
    {
        if (Status == CouponStatus.Active && UsedCount > 0)
            throw new DomainException("Cannot update an active coupon that has been used");

        if (endDate <= startDate)
            throw new DomainException("End date must be after start date");

        Name = name;
        Description = description;
        Discount = discount ?? throw new DomainException("Discount is required");
        StartDate = startDate;
        EndDate = endDate;
        MinOrderValue = minOrderValue;
        MaxDiscountAmount = maxDiscountAmount;
        UsageLimitTotal = usageLimitTotal;
        UsageLimitPerCustomer = usageLimitPerCustomer;

        IncreaseVersion();
    }

    public void CheckAndExpire(DateTimeOffset? currentTime = null)
    {
        var now = currentTime ?? DateTimeOffset.UtcNow;
        if (now > EndDate && Status == CouponStatus.Active)
        {
            Status = CouponStatus.Expired;
            AddEvent(new CouponExpiredEvent(Id, Code));
            IncreaseVersion();
        }
    }
}
