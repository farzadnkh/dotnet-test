using ExchangeRateProvider.Application.Consumers.Extensions;
using ExchangeRateProvider.Contract.Commons.Dtos;
using FluentAssertions;

namespace ExchangeRateProvider.Domain.UnitTest.ExchangeRateProviders;

public class ExchangeRateFilterHelperTests
{
    private ExchangeRateValue CreateRate(decimal price, long ticksOffset)
    {
        return new ExchangeRateValue
        {
            Price = price,
            Ticks = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ticksOffset
        };
    }
    [Fact]
    public void FilterOutliers_WithNullList_ShouldReturnEmptyList()
    {
        // Arrange
        List<ExchangeRateValue> rates = null;

        // Act
        var result = ExchangeRateFilterHelper.FilterOutliers(rates, 1000, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FilterOutliers_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var rates = new List<ExchangeRateValue>();

        // Act
        var result = ExchangeRateFilterHelper.FilterOutliers(rates, 1000, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FilterOutliers_WithSingleRate_ShouldReturnTheSameRate()
    {
        // Arrange
        var rate = CreateRate(1.2m, 100);
        var rates = new List<ExchangeRateValue> { rate };

        // Act
        var result = ExchangeRateFilterHelper.FilterOutliers(rates, 1000, 10);

        // Assert
        result.Should().ContainSingle().And.Equal(rate);
    }

    [Fact]
    public void FilterOutliers_WithRatesOlderThanMaxAge_ShouldFilterThemOut()
    {
        // Arrange
        var oldRate = CreateRate(1.2m, 20000);
        var newRate = CreateRate(1.3m, 5000);
        var rates = new List<ExchangeRateValue> { oldRate, newRate };

        // Act
        var result = ExchangeRateFilterHelper.FilterOutliers(rates, 10000, 10);

        // Assert
        result.Should().ContainSingle().And.Equal(newRate);
    }

    [Fact]
    public void FilterOutliers_WithTwoRatesAndNoOutlier_ShouldReturnBoth()
    {
        // Arrange
        var rate1 = CreateRate(1.5m, 100);
        var rate2 = CreateRate(1.6m, 200);
        var rates = new List<ExchangeRateValue> { rate1, rate2 };

        // Act
        var result = ExchangeRateFilterHelper.FilterOutliers(rates, 1000, 10);

        // Assert
        result.Should().HaveCount(2).And.Contain(rate1).And.Contain(rate2);
    }

    [Fact]
    public void FilterOutliers_WithTwoRatesAndAnOutlier_ShouldReturnMostRecent()
    {
        // Arrange
        var outlier = CreateRate(1.5m, 200);
        var recentRate = CreateRate(1.8m, 100);
        var rates = new List<ExchangeRateValue> { outlier, recentRate };
        const decimal maxRatioDiff = 10m; // 10% diff

        // Act
        var result = ExchangeRateFilterHelper.FilterOutliers(rates, 1000, maxRatioDiff);

        // Assert
        result.Should().ContainSingle().And.Equal(recentRate);
    }

    [Fact]
    public void FilterOutliers_WithThreeRatesAndOneOutlier_ShouldReturnTwoCorrectRates()
    {
        // Arrange
        var outlierRate = CreateRate(10m, 100);     // Outlier
        var medianRate = CreateRate(100m, 200);
        var validRate = CreateRate(105m, 300);
        var rates = new List<ExchangeRateValue> { outlierRate, medianRate, validRate };
        const decimal maxRatioDiff = 10m; // 10% diff

        // Act
        var result = ExchangeRateFilterHelper.FilterOutliers(rates, 1000, maxRatioDiff);

        // Assert
        result.Should().HaveCount(2).And.Contain(medianRate).And.Contain(validRate);
        result.Should().NotContain(outlierRate);
    }

    [Fact]
    public void FilterOutliers_WithThreeRatesAndNoOutlier_ShouldReturnAllThree()
    {
        // Arrange
        var rates = new List<ExchangeRateValue>
        {
            CreateRate(100m, 100),
            CreateRate(101m, 200),
            CreateRate(102m, 300)
        };
        const decimal maxRatioDiff = 5m; // 5% diff

        // Act
        var result = ExchangeRateFilterHelper.FilterOutliers(rates, 1000, maxRatioDiff);

        // Assert
        result.Should().HaveCount(3).And.Contain(rates);
    }

    [Fact]
    public void FilterOutliers_WithManyRatesAndOutliers_ShouldUseIqrMethod()
    {
        // Arrange
        var outlierPrice1 = CreateRate(50m, 100);
        var validRate1 = CreateRate(100m, 200);
        var validRate2 = CreateRate(105m, 300);
        var validRate3 = CreateRate(110m, 400);
        var outlierPrice2 = CreateRate(250m, 500);
        var outlierAge = CreateRate(115m, 2000000);

        var rates = new List<ExchangeRateValue>
        {
            outlierPrice1, validRate1, validRate2, validRate3, outlierPrice2, outlierAge
        };

        // Act
        var result = ExchangeRateFilterHelper.FilterOutliers(rates, 10000, 10);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(validRate1).And.Contain(validRate2).And.Contain(validRate3);
        result.Should().NotContain(outlierPrice1).And.NotContain(outlierPrice2).And.NotContain(outlierAge);
    }

    [Fact]
    public void FilterOutliers_WithNegativeOrZeroMaxAllowedRatio_ShouldUseDefaultValue()
    {
        // Arrange
        var outlier = CreateRate(1.5m, 200);
        var recentRate = CreateRate(1.8m, 100);
        var rates = new List<ExchangeRateValue> { outlier, recentRate };

        // Act
        var result = ExchangeRateFilterHelper.FilterOutliers(rates, 1000, 0);
        var result2 = ExchangeRateFilterHelper.FilterOutliers(rates, 1000, -5);

        // Assert
        result.Should().ContainSingle().And.Equal(recentRate);
        result2.Should().ContainSingle().And.Equal(recentRate);
    }
}
