using ExchangeRateProvider.Domain.Users.Entities;

namespace ExchangeRateProvider.Domain.Settings.Entities;

public class Setting : Entity<int>
{
    #region Ctor

    public Setting() { }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="name">Name</param>
    /// <param name="value">Value</param>
    /// <param name="userRefId">User identifier</param>
    public Setting(string name, string value, int? userId = null)
    {
        Name = name;
        Value = value;
        UserId = userId;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the user for which this setting is valid. 
    /// null is set when the setting is for site.
    /// 1 is set when the setting is for default setting.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value
    /// </summary>
    public string Value { get; set; }

    #endregion

    #region Relation Properties

    /// <summary>
    /// Gets or sets the user entity
    /// </summary>
    public User User { get; set; }

    #endregion

    /// <summary>
    /// To string
    /// </summary>
    /// <returns>Result</returns>
    public override string ToString()
    {
        return Name;
    }
}
