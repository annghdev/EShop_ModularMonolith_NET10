namespace Contracts.Responses;

public class MoneyDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
}
