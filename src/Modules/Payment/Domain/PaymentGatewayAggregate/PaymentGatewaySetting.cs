namespace Payment.Domain;

public class PaymentGatewaySetting : BaseEntity
{
    public Guid PaymentGatewayId { get; private set; }
    public PaymentGateway? PaymentGateway { get; private set; }
    public string ApiKey { get; private set; } = string.Empty;
    public string SecretKey { get; private set; } = string.Empty;
    public string? MerchantId { get; private set; }
    public string? ReturnUrl { get; private set; }
    public string? IpnUrl { get; private set; }
    public Dictionary<string, string>? AdditionalSettings { get; private set; }

    private PaymentGatewaySetting() { } // EF Core

    internal PaymentGatewaySetting(
        string apiKey,
        string secretKey,
        string? merchantId = null,
        string? returnUrl = null,
        string? ipnUrl = null,
        Dictionary<string, string>? additionalSettings = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new DomainException("API Key is required");

        if (string.IsNullOrWhiteSpace(secretKey))
            throw new DomainException("Secret Key is required");

        ApiKey = apiKey;
        SecretKey = secretKey;
        MerchantId = merchantId;
        ReturnUrl = returnUrl;
        IpnUrl = ipnUrl;
        AdditionalSettings = additionalSettings;
    }

    internal void Update(
        string apiKey,
        string secretKey,
        string? merchantId = null,
        string? returnUrl = null,
        string? ipnUrl = null,
        Dictionary<string, string>? additionalSettings = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new DomainException("API Key is required");

        if (string.IsNullOrWhiteSpace(secretKey))
            throw new DomainException("Secret Key is required");

        ApiKey = apiKey;
        SecretKey = secretKey;
        MerchantId = merchantId;
        ReturnUrl = returnUrl;
        IpnUrl = ipnUrl;
        AdditionalSettings = additionalSettings;
    }
}
