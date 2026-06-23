namespace BlogPlatform.Domain.Common;

public abstract class Entity
{
    private readonly List<string> _notifications = [];

    public Guid Id { get; protected set; }

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public IReadOnlyList<string> Notifications => _notifications;

    public bool IsValid => _notifications.Count == 0;

    public void AddNotification(string message) => _notifications.Add(message);

    public void AddNotifications(IEnumerable<string> messages) => _notifications.AddRange(messages);

    public void ThrowIfInvalid()
    {
        if (IsValid) return;
        throw new DomainValidationException(_notifications);
    }
}
