namespace Shipping.Domain;

public class ShippingCarrierSetting : BaseEntity
{
    public string ApiToken { get; private set; } = string.Empty;
    public string? ShopId { get; private set; }
    public string? ApiUrl { get; private set; }
    public string? WebhookUrl { get; private set; }
    public Dictionary<string, string>? AdditionalSettings { get; private set; }

    private ShippingCarrierSetting() { } // EF Core

    public ShippingCarrierSetting(
        string apiToken,
        string? shopId = null,
        string? apiUrl = null,
        string? webhookUrl = null,
        Dictionary<string, string>? additionalSettings = null)
    {
        if (string.IsNullOrWhiteSpace(apiToken))
            throw new DomainException("API Token is required");

        ApiToken = apiToken;
        ShopId = shopId;
        ApiUrl = apiUrl;
        WebhookUrl = webhookUrl;
        AdditionalSettings = additionalSettings;
    }

    public void Update(
        string apiToken,
        string? shopId = null,
        string? apiUrl = null,
        string? webhookUrl = null,
        Dictionary<string, string>? additionalSettings = null)
    {
        if (string.IsNullOrWhiteSpace(apiToken))
            throw new DomainException("API Token is required");

        ApiToken = apiToken;
        ShopId = shopId;
        ApiUrl = apiUrl;
        WebhookUrl = webhookUrl;
        AdditionalSettings = additionalSettings;
    }

    public string? GetSetting(string key)
    {
        return AdditionalSettings?.TryGetValue(key, out var value) == true ? value : null;
    }
}
