using Microsoft.Extensions.Hosting;
using Moq;
using NT.KYC.Jibit.HashiCorpVault.Extensions;

namespace Test.NT.KYC.Jibit;

public class HostEnvironmentExtensionsTests
{
    [Theory]
    [InlineData("Debug", true)]
    [InlineData("debug", true)]
    [InlineData("Production", false)]
    public void IsDebugMode_ReturnsTrueOnlyForDebugEnvironment(string environmentName, bool expected)
    {
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(e => e.EnvironmentName).Returns(environmentName);

        var result = environment.Object.IsDebugMode();

        Assert.Equal(expected, result);
    }
}
