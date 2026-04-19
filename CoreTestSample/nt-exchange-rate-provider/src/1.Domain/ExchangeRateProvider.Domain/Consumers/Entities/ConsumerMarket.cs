using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Domain.Users.Entities;
using NT.DDD.Domain.Auditing;

namespace ExchangeRateProvider.Domain.Consumers.Entities;

public class ConsumerMarket : AuditedEntity<long, User, int>
{
    #region Properties
    public int ConsumerId { get; set; }
    public Consumer Consumer { get; set; }

    public int MarketId { get; set; }
    public Market Market { get; set; }

    public bool IsActive { get; set; }
    public SpreadOptions SpreadOptions { get; private set; } = new();
    public int CreatedById { get; private set; }

    #endregion

    #region Constructors

    public ConsumerMarket(int consumerId, int marketId, bool isActive, int createdById, DateTimeOffset createdOnUtc)
    {
        ConsumerId = consumerId;
        MarketId = marketId;
        IsActive = isActive;
        CreatedById = createdById;
        CreatedOnUtc = createdOnUtc;
    }

    public ConsumerMarket()
    {
    }

    #endregion

    #region Behaviors

    /// <summary>
    /// Enables the spread limits for this consumer-market relationship.
    /// </summary>
    public void EnableSpread()
    {
        SpreadOptions.SpreadEnabled = true;
    }

    /// <summary>
    /// Disables the spread limits for this consumer-market relationship.
    /// </summary>
    public void DisableSpread()
    {
        SpreadOptions.SpreadEnabled = false;
        SpreadOptions.LowerLimitPercentage = null;
        SpreadOptions.UpperLimitPercentage = null;
    }

    /// <summary>
    /// Sets the lower and upper spread limit percentages.
    /// </summary>
    /// <param name="lowerPercentage">The lower limit percentage.</param>
    /// <param name="upperPercentage">The upper limit percentage.</param>
    /// <exception cref="ArgumentException">Thrown if lowerPercentage is greater than or equal to upperPercentage.</exception>
    public void SetSpreadLimits(SpreadOptions spreadOptions)
    {
        if (spreadOptions.LowerLimitPercentage >= spreadOptions.UpperLimitPercentage)
        {
            throw new ArgumentException("Lower limit percentage must be less than the upper limit percentage.");
        }
        
        SpreadOptions.SpreadEnabled = spreadOptions.SpreadEnabled;
        if(spreadOptions.SpreadEnabled)
        {
            SpreadOptions.LowerLimitPercentage = spreadOptions.LowerLimitPercentage;
            SpreadOptions.UpperLimitPercentage = spreadOptions.UpperLimitPercentage;
        }
        else
        {
            SpreadOptions.LowerLimitPercentage = null;
            SpreadOptions.UpperLimitPercentage = null;
        }

        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

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
