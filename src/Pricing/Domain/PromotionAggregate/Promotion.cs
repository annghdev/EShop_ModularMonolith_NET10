namespace Pricing.Domain;

public class Promotion : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public PromotionType Type { get; private set; }
    public int Priority { get; private set; }
    public bool IsStackable { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public PromotionStatus Status { get; private set; }

    private readonly List<PromotionRule> _rules = [];
    public IReadOnlyCollection<PromotionRule> Rules => _rules.AsReadOnly();

    private readonly List<PromotionAction> _actions = [];
    public IReadOnlyCollection<PromotionAction> Actions => _actions.AsReadOnly();

    private Promotion() { } // EF Core

    public static Promotion Create(
        string name,
        string? description,
        PromotionType type,
        int priority,
        bool isStackable,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Promotion name is required");

        if (endDate <= startDate)
            throw new DomainException("End date must be after start date");

        if (priority < 0)
            throw new DomainException("Priority cannot be negative");

        return new Promotion
        {
            Name = name,
            Description = description,
            Type = type,
            Priority = priority,
            IsStackable = isStackable,
            StartDate = startDate,
            EndDate = endDate,
            Status = PromotionStatus.Draft
        };
    }

    public void UpdateDetails(
        string name,
        string? description,
        PromotionType type,
        int priority,
        bool isStackable,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        if (Status == PromotionStatus.Active)
            throw new DomainException("Cannot update an active promotion");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Promotion name is required");

        if (endDate <= startDate)
            throw new DomainException("End date must be after start date");

        Name = name;
        Description = description;
        Type = type;
        Priority = priority;
        IsStackable = isStackable;
        StartDate = startDate;
        EndDate = endDate;

        IncreaseVersion();
    }

    public void Activate()
    {
        if (Status == PromotionStatus.Ended)
            throw new DomainException("Cannot activate an ended promotion");

        if (!_rules.Any())
            throw new DomainException("Promotion must have at least one rule");

        if (!_actions.Any())
            throw new DomainException("Promotion must have at least one action");

        Status = PromotionStatus.Active;
        AddEvent(new PromotionActivatedEvent(Id, Name, Type));
        IncreaseVersion();
    }

    public void Deactivate()
    {
        Status = PromotionStatus.Disabled;
        AddEvent(new PromotionDeactivatedEvent(Id, Name));
        IncreaseVersion();
    }

    public void End()
    {
        Status = PromotionStatus.Ended;
        AddEvent(new PromotionEndedEvent(Id, Name));
        IncreaseVersion();
    }

    public void AddRule(
        RuleType type,
        Guid? targetId = null,
        int? minQuantity = null,
        Money? minOrderValue = null,
        string? customerTier = null)
    {
        if (Status == PromotionStatus.Active)
            throw new DomainException("Cannot modify rules of an active promotion");

        var rule = new PromotionRule(type, targetId, minQuantity, minOrderValue, customerTier);
        _rules.Add(rule);
        IncreaseVersion();
    }

    public void RemoveRule(Guid ruleId)
    {
        if (Status == PromotionStatus.Active)
            throw new DomainException("Cannot modify rules of an active promotion");

        var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
        if (rule != null)
        {
            _rules.Remove(rule);
            IncreaseVersion();
        }
    }

    public void AddAction(
        ActionType type,
        DiscountValue? discount = null,
        Guid? giftProductId = null,
        Guid? giftVariantId = null,
        int giftQuantity = 1)
    {
        if (Status == PromotionStatus.Active)
            throw new DomainException("Cannot modify actions of an active promotion");

        var action = new PromotionAction(type, discount, giftProductId, giftVariantId, giftQuantity);
        _actions.Add(action);
        IncreaseVersion();
    }

    public void RemoveAction(Guid actionId)
    {
        if (Status == PromotionStatus.Active)
            throw new DomainException("Cannot modify actions of an active promotion");

        var action = _actions.FirstOrDefault(a => a.Id == actionId);
        if (action != null)
        {
            _actions.Remove(action);
            IncreaseVersion();
        }
    }

    public bool IsActive(DateTimeOffset? currentTime = null)
    {
        var now = currentTime ?? DateTimeOffset.UtcNow;
        return Status == PromotionStatus.Active && now >= StartDate && now <= EndDate;
    }

    public void CheckAndEnd(DateTimeOffset? currentTime = null)
    {
        var now = currentTime ?? DateTimeOffset.UtcNow;
        if (now > EndDate && Status == PromotionStatus.Active)
        {
            End();
        }
    }

    /// <summary>
    /// Gets all discount actions from this promotion
    /// </summary>
    public IEnumerable<PromotionAction> GetDiscountActions()
    {
        return _actions.Where(a =>
            a.Type == ActionType.PercentageDiscount ||
            a.Type == ActionType.FixedAmountDiscount);
    }

    /// <summary>
    /// Gets all gift product actions from this promotion
    /// </summary>
    public IEnumerable<PromotionAction> GetGiftActions()
    {
        return _actions.Where(a => a.Type == ActionType.GiftProduct);
    }

    /// <summary>
    /// Checks if this promotion has free shipping action
    /// </summary>
    public bool HasFreeShipping()
    {
        return _actions.Any(a => a.Type == ActionType.FreeShipping);
    }
}
