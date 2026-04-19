using Newtonsoft.Json;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Responses;

namespace Test.NT.KYC.Jibit;

public class SingleOrArrayConverterTests
{
    [Fact]
    public void Deserialize_WhenDataIsArray_ReturnsAllItems()
    {
        var payload = """
        {
          "data": [
            { "status": true },
            { "status": false }
          ]
        }
        """;

        var result = JsonConvert.DeserializeObject<JibitAlphaResponseWrapper<OneStepKycResponse>>(payload);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Collection(result.Data,
            first => Assert.True(first.Status),
            second => Assert.False(second.Status));
    }

    [Fact]
    public void Deserialize_WhenDataIsSingleObject_WrapsInList()
    {
        var payload = """
        {
          "data": { "status": true }
        }
        """;

        var result = JsonConvert.DeserializeObject<JibitAlphaResponseWrapper<OneStepKycResponse>>(payload);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        var item = Assert.Single(result.Data);
        Assert.True(item.Status);
    }
}
