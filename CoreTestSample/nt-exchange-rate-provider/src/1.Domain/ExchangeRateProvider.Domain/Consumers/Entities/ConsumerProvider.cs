using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.Users.Entities;
using NT.DDD.Domain.Auditing;

namespace ExchangeRateProvider.Domain.Consumers.Entities;

public class ConsumerProvider : AuditedEntity<long, User, int>
{
    #region Properties
    public int ConsumerId { get; set; }
    public Consumer Consumer { get; set; }

    public int ProviderId { get; set; }
    public Provider Provider { get; set; }

    public bool IsActive { get; set; }
    public int CreatedById { get; private set; }

    #endregion

    #region Constructors

    public ConsumerProvider(int consumerId, int providerId, bool isActive, int createdById, DateTimeOffset createdOnUtc)
    {
        ConsumerId = consumerId;
        ProviderId = providerId;
        IsActive = isActive;
        CreatedById = createdById;
        CreatedOnUtc = createdOnUtc;
    }

    public ConsumerProvider()
    {
    }

    #endregion

    #region Behaviors

    /// <summary>
    /// Marks this consumer-market relationship as inactive.
    /// </summary>
    public void MarkAsInactive()
    {
        IsActive = false;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks this consumer-market relationship as active.
    /// </summary>
    public void MarkAsActive()
    {
        IsActive = true;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the last modified user and timestamp.
    /// </summary>
    /// <param name="modifierUserId">The ID of the user performing the modification.</param>
    public void UpdateModifiedInfo(int modifierUserId)
    {
        LastModifierUserId = modifierUserId;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    #endregion
}
