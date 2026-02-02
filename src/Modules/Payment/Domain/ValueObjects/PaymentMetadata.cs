namespace Payment.Domain;

public class PaymentMetadata : BaseValueObject
{
    public string? ExternalTransactionId { get; }
    public string? BankCode { get; }
    public string? CardType { get; }
    public string? ResponseCode { get; }
    public string? ResponseMessage { get; }
    public string? IpAddress { get; }
    public Dictionary<string, string>? AdditionalData { get; }

    private PaymentMetadata() { } // EF Core

    public PaymentMetadata(
        string? externalTransactionId = null,
        string? bankCode = null,
        string? cardType = null,
        string? responseCode = null,
        string? responseMessage = null,
        string? ipAddress = null,
        Dictionary<string, string>? additionalData = null)
    {
        ExternalTransactionId = externalTransactionId;
        BankCode = bankCode;
        CardType = cardType;
        ResponseCode = responseCode;
        ResponseMessage = responseMessage;
        IpAddress = ipAddress;
        AdditionalData = additionalData;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ExternalTransactionId;
        yield return BankCode;
        yield return CardType;
        yield return ResponseCode;
        yield return ResponseMessage;
        yield return IpAddress;
    }

    public PaymentMetadata WithExternalTransactionId(string externalTransactionId)
        => new(externalTransactionId, BankCode, CardType, ResponseCode, ResponseMessage, IpAddress, AdditionalData);

    public PaymentMetadata WithResponseData(string responseCode, string responseMessage)
        => new(ExternalTransactionId, BankCode, CardType, responseCode, responseMessage, IpAddress, AdditionalData);

    public PaymentMetadata WithBankInfo(string bankCode, string? cardType = null)
        => new(ExternalTransactionId, bankCode, cardType, ResponseCode, ResponseMessage, IpAddress, AdditionalData);
}
