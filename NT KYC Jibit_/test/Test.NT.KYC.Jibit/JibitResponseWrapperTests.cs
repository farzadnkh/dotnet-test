using System.Net;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Responses;
using NT.SDK.RestClient.Models;

namespace Test.NT.KYC.Jibit;

public class JibitResponseWrapperTests
{
    [Fact]
    public void AddJibitErrorResponse_WithErrorPayload_PopulatesErrors()
    {
        var wrapper = new JibitResponseWrapper<GetAccessTokenResponse>();
        var rawContent = """
        {
          "code": "forbidden",
          "message": "access denied"
        }
        """;
        var response = new ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>(
            HttpStatusCode.BadRequest,
            wrapper,
            rawContent);

        wrapper.AddJibitErrorResponse(response);

        Assert.NotNull(wrapper.Errors);
        var error = Assert.Single(wrapper.Errors);
        Assert.Equal("forbidden", error.Code);
        Assert.Equal("access denied", error.Message);
    }

    [Fact]
    public void AddJibitErrorResponse_WhenDataExists_DoesNothing()
    {
        var wrapper = new JibitResponseWrapper<GetAccessTokenResponse>
        {
            Data = new GetAccessTokenResponse { AccessToken = "token" }
        };
        var response = new ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>(
            HttpStatusCode.BadRequest,
            wrapper,
            "{\"code\":\"forbidden\",\"message\":\"denied\"}");

        wrapper.AddJibitErrorResponse(response);

        Assert.Null(wrapper.Errors);
    }

    [Fact]
    public void TransferToObject_DeserializesResponseBody()
    {
        var wrapper = new JibitResponseWrapper<GetAccessTokenResponse>();
        var payload = """
        {
          "accessToken": "access",
          "refreshToken": "refresh"
        }
        """;

        wrapper.TransferToObject(payload);

        Assert.NotNull(wrapper.Data);
        Assert.Equal("access", wrapper.Data.AccessToken);
        Assert.Equal("refresh", wrapper.Data.RefreshToken);
    }
}
