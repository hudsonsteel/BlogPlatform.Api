namespace BlogPlatform.Domain.Common;

public sealed class DomainValidationException(IReadOnlyList<string> notifications)
    : Exception("Domain validation failed.")
{
    public IReadOnlyList<string> Notifications { get; } = notifications;
}
