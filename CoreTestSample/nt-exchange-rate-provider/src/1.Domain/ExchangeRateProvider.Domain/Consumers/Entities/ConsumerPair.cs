using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Domain.Users.Entities;
using NT.DDD.Domain.Auditing;

namespace ExchangeRateProvider.Domain.Consumers.Entities
{
    public class ConsumerPair : AuditedEntity<long, User, int>
    {
        #region Properties

        public int ConsumerId { get; set; }
        public Consumer Consumer { get; set; }

        public int MarketId { get; set; }
        public Market Market { get; set; }

        public int PairId { get; set; }
        public MarketTradingPair MarketTradingPair { get; set; }

        public bool IsActive { get; set; }
        public SpreadOptions SpreadOptions { get; private set; } = new();
        public int CreatedById { get; private set; }

        #endregion


        #region Constructors

        public ConsumerPair(int consumerId, int marketId, int pairId, bool isActive, int createdById)
        {
            ConsumerId = consumerId;
            MarketId = marketId;
            PairId = pairId;
            IsActive = isActive;
            CreatedById = createdById;
            CreatedOnUtc = DateTime.UtcNow;
        }

        public ConsumerPair()
        {
            CreatedOnUtc = DateTimeOffset.UtcNow;
        }

        #endregion

        #region Behaviors

        public void EnableSpread()
        {
            SpreadOptions.SpreadEnabled = true;
        }

        public void DisableSpread()
        {
            SpreadOptions.SpreadEnabled = false;
            SpreadOptions.LowerLimitPercentage = null;
            SpreadOptions.UpperLimitPercentage = null;
        }

        public void SetSpreadLimits(SpreadOptions spreadOptions)
        {
            if (spreadOptions.LowerLimitPercentage >= spreadOptions.UpperLimitPercentage)
            {
                throw new ArgumentException("Lower limit percentage must be less than the upper limit percentage.");
            }

            SpreadOptions.SpreadEnabled = spreadOptions.SpreadEnabled;
            if (spreadOptions.SpreadEnabled)
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

        public void MarkAsInactive()
        {
            IsActive = false;
            UpdatedOnUtc = DateTimeOffset.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedOnUtc = DateTimeOffset.UtcNow;
        }

        public void UpdateModifiedInfo(int modifierUserId)
        {
            LastModifierUserId = modifierUserId;
            UpdatedOnUtc = DateTimeOffset.UtcNow;
        }

        #endregion
    }
}
