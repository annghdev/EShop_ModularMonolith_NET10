namespace Payment.Domain;

public class PaymentGateway : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public PaymentProvider Provider { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsEnabled { get; private set; }
    public int DisplayOrder { get; private set; }
    public PaymentGatewayStatus Status { get; private set; }

    private readonly List<string> _supportedCurrencies = [];
    public IReadOnlyCollection<string> SupportedCurrencies => _supportedCurrencies.AsReadOnly();

    public PaymentGatewaySetting? Setting { get; private set; }

    private PaymentGateway() { } // EF Core

    public static PaymentGateway Create(
        string name,
        PaymentProvider provider,
        string displayName,
        string? description = null,
        int displayOrder = 0,
        List<string>? supportedCurrencies = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Gateway name is required");

        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("Display name is required");

        var gateway = new PaymentGateway
        {
            Name = name,
            Provider = provider,
            DisplayName = displayName,
            Description = description,
            IsEnabled = false,
            DisplayOrder = displayOrder,
            Status = PaymentGatewayStatus.Active
        };

        if (supportedCurrencies != null && supportedCurrencies.Any())
        {
            gateway._supportedCurrencies.AddRange(supportedCurrencies);
        }
        else
        {
            // Default to VND
            gateway._supportedCurrencies.Add("VND");
        }

        return gateway;
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
        string apiKey,
        string secretKey,
        string? merchantId = null,
        string? returnUrl = null,
        string? ipnUrl = null,
        Dictionary<string, string>? additionalSettings = null)
    {
        if (Provider == PaymentProvider.COD)
            throw new DomainException("COD payment method does not require configuration");

        if (Setting == null)
        {
            Setting = new PaymentGatewaySetting(
                apiKey, secretKey, merchantId, returnUrl, ipnUrl, additionalSettings);
        }
        else
        {
            Setting.Update(apiKey, secretKey, merchantId, returnUrl, ipnUrl, additionalSettings);
        }

        AddEvent(new PaymentGatewayConfigurationUpdatedEvent(Id, Name));
        IncreaseVersion();
    }

    public void Enable()
    {
        if (Status == PaymentGatewayStatus.Maintenance)
            throw new DomainException("Cannot enable gateway in maintenance mode");

        if (Provider != PaymentProvider.COD && Setting == null)
            throw new DomainException("Gateway must be configured before enabling");

        IsEnabled = true;
        Status = PaymentGatewayStatus.Active;

        AddEvent(new PaymentGatewayEnabledEvent(Id, Name, Provider));
        IncreaseVersion();
    }

    public void Disable()
    {
        IsEnabled = false;
        Status = PaymentGatewayStatus.Disabled;

        AddEvent(new PaymentGatewayDisabledEvent(Id, Name, Provider));
        IncreaseVersion();
    }

    public void SetMaintenance()
    {
        IsEnabled = false;
        Status = PaymentGatewayStatus.Maintenance;

        IncreaseVersion();
    }

    public void AddSupportedCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency code is required");

        if (_supportedCurrencies.Contains(currency))
            return;

        _supportedCurrencies.Add(currency.ToUpperInvariant());
        IncreaseVersion();
    }

    public void RemoveSupportedCurrency(string currency)
    {
        if (_supportedCurrencies.Count == 1)
            throw new DomainException("Gateway must support at least one currency");

        _supportedCurrencies.Remove(currency);
        IncreaseVersion();
    }

    public bool SupportsCurrency(string currency)
    {
        return _supportedCurrencies.Contains(currency, StringComparer.OrdinalIgnoreCase);
    }

    public void ValidateConfiguration()
    {
        if (Provider == PaymentProvider.COD)
            return; // COD doesn't need configuration

        if (Setting == null)
            throw new DomainException("Gateway configuration is required");

        if (string.IsNullOrWhiteSpace(Setting.ApiKey))
            throw new DomainException("API Key is required");

        if (string.IsNullOrWhiteSpace(Setting.SecretKey))
            throw new DomainException("Secret Key is required");
    }
}
