namespace Contracts;

public record AuthResult(string AccessToken, string? RefreshToken = null)
    : TokensResult(AccessToken, RefreshToken)
{
    public UserInfo? UserInfo { get; init; }
}

public record TokensResult(string AccessToken, string? RefreshToken = null);

public record UserInfo
{
    public string DisplayName { get; init; } = string.Empty;
    public string AvatarUrl { get; init; } = string.Empty;
    public string DisplayRole { get; init; } = string.Empty;
    public string? PersonalSetting { get; init; }
}