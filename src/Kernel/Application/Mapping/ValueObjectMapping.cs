using Kernel.Domain;

namespace Kernel.Application;

public static class ValueOnjectMapping
{
    public static Money? ToMoney(this MoneyDto? dto) 
        => dto != null ? new(dto.Amount, dto.Currency) : null;

    public static MoneyDto? ToMoneyDto(this Money? money)
        => money != null ? new(money.Amount, money.Currency) : null;

    public static Dimensions? ToDimensions(this DimensionsDto? dto)
        => dto != null ? new(dto.Width, dto.Height, dto.Depth, dto.Weight) : null;

    public static DimensionsDto? ToDimensionsDto(this Dimensions? dimensions)
        => dimensions != null ? new(dimensions.Width, dimensions.Height, dimensions.Depth, dimensions.Weight): null;
}