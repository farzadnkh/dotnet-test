using System.Net;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Logger;

namespace Test.NT.KYC.Jibit;

public class JibitClientConfigurationTests
{
    [Fact]
    public void JibitRestClientConfiguration_AllowsUpdatingApiKeyAndOptionalSettings()
    {
        var configuration = new JibitRestClientConfiguration(
            "initial",
            "secret",
            "https://api",
            4000,
            _ => { });

        configuration.SetApiKey("updated");
        configuration.Proxy = new WebProxy("https://proxy");
        configuration.DefaultHeaders = new Dictionary<string, string> { { "x-test", "value" } };
        configuration.DateTimeFormat = "yyyy-MM-dd";
        configuration.TempFolderPath = "/tmp";
        configuration.ClientCertificates = new();

        Assert.Equal("updated", configuration.ApiKey);
        Assert.Equal("secret", configuration.Password);
        Assert.Equal("https://api", configuration.BasePath);
        Assert.Equal(4000, configuration.Timeout);
        Assert.True(configuration.Proxy?.Address?.AbsoluteUri!.Contains("proxy"));
        Assert.Equal("value", configuration.DefaultHeaders?["x-test"]);
        Assert.Equal("yyyy-MM-dd", configuration.DateTimeFormat);
        Assert.Equal("/tmp", configuration.TempFolderPath);
        Assert.NotNull(configuration.ClientCertificates);
    }

    [Fact]
    public void JibitAlphaRestClientConfiguration_TracksPermanentTokenAndApiKey()
    {
        var configuration = new JibitAlphaRestClientConfiguration(
            "permanent",
            "https://alpha",
            5000,
            _ => { });

        configuration.SetApiKey("temporary");
        configuration.Proxy = new WebProxy("https://proxy");
        configuration.DefaultHeaders = new Dictionary<string, string> { { "x-alpha", "value" } };

        Assert.Equal("temporary", configuration.ApiKey);
        Assert.Equal("permanent", configuration.PermanentToken);
        Assert.Equal("https://alpha", configuration.BasePath);
        Assert.Equal(5000, configuration.Timeout);
        Assert.Equal("value", configuration.DefaultHeaders?["x-alpha"]);
    }
}
