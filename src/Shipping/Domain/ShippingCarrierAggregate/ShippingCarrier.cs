namespace Shipping.Domain;

public class ShippingCarrier : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public ShippingProvider Provider { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsEnabled { get; private set; }
    public int DisplayOrder { get; private set; }
    public ShippingCarrierStatus Status { get; private set; }

    private readonly List<string> _supportedRegions = [];
    public IReadOnlyCollection<string> SupportedRegions => _supportedRegions.AsReadOnly();

    public ShippingCarrierSetting? Setting { get; private set; }

    private ShippingCarrier() { } // EF Core

    public static ShippingCarrier Create(
        string name,
        ShippingProvider provider,
        string displayName,
        string? description = null,
        int displayOrder = 0,
        List<string>? supportedRegions = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Carrier name is required");

        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("Display name is required");

        var carrier = new ShippingCarrier
        {
            Name = name,
            Provider = provider,
            DisplayName = displayName,
            Description = description,
            IsEnabled = false,
            DisplayOrder = displayOrder,
            Status = ShippingCarrierStatus.Active
        };

        if (supportedRegions != null && supportedRegions.Any())
        {
            carrier._supportedRegions.AddRange(supportedRegions);
        }

        return carrier;
    }

    public void UpdateDisplaySettings(string displayName, string? description = null, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("Display name is required");

        DisplayName = displayName;
        Description = description;
        DisplayOrder = displayOrder;

        IncreaseVersion();
    }

    public void UpdateConfiguration(
        string apiToken,
        string? shopId = null,
        string? apiUrl = null,
        string? webhookUrl = null,
        Dictionary<string, string>? additionalSettings = null)
    {
        if (Provider == ShippingProvider.Manual)
            throw new DomainException("Manual shipping does not require configuration");

        if (Setting == null)
        {
            Setting = new ShippingCarrierSetting(
                apiToken, shopId, apiUrl, webhookUrl, additionalSettings);
        }
        else
        {
            Setting.Update(apiToken, shopId, apiUrl, webhookUrl, additionalSettings);
        }

        AddEvent(new ShippingCarrierConfigurationUpdatedEvent(Id, Name));
        IncreaseVersion();
    }

    public void Enable()
    {
        if (Status == ShippingCarrierStatus.Maintenance)
            throw new DomainException("Cannot enable carrier in maintenance mode");

        if (Provider != ShippingProvider.Manual && Setting == null)
            throw new DomainException("Carrier must be configured before enabling");

        IsEnabled = true;
        Status = ShippingCarrierStatus.Active;

        AddEvent(new ShippingCarrierEnabledEvent(Id, Name, Provider));
        IncreaseVersion();
    }

    public void Disable()
    {
        IsEnabled = false;
        Status = ShippingCarrierStatus.Disabled;

        AddEvent(new ShippingCarrierDisabledEvent(Id, Name, Provider));
        IncreaseVersion();
    }

    public void SetMaintenance()
    {
        IsEnabled = false;
        Status = ShippingCarrierStatus.Maintenance;

        IncreaseVersion();
    }

    public void AddSupportedRegion(string region)
    {
        if (string.IsNullOrWhiteSpace(region))
            throw new DomainException("Region is required");

        if (_supportedRegions.Contains(region, StringComparer.OrdinalIgnoreCase))
            return;

        _supportedRegions.Add(region);
        IncreaseVersion();
    }

    public void RemoveSupportedRegion(string region)
    {
        _supportedRegions.RemoveAll(r => r.Equals(region, StringComparison.OrdinalIgnoreCase));
        IncreaseVersion();
    }

    public bool SupportsRegion(string region)
    {
        // If no regions specified, supports all regions
        if (!_supportedRegions.Any())
            return true;

        return _supportedRegions.Contains(region, StringComparer.OrdinalIgnoreCase);
    }

    public void ValidateConfiguration()
    {
        if (Provider == ShippingProvider.Manual)
            return; // Manual doesn't need configuration

        if (Setting == null)
            throw new DomainException("Carrier configuration is required");

        if (string.IsNullOrWhiteSpace(Setting.ApiToken))
            throw new DomainException("API Token is required");
    }
}
