using System.Net;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Responses;
using NT.SDK.RestClient.Models;

namespace Test.NT.KYC.Jibit;

public class JibitAlphaResponseWrapperTests
{
    [Fact]
    public void AddError_WhenWrapperHasErrorMessage_AddsErrorFromWrapper()
    {
        var wrapper = new JibitAlphaResponseWrapper<OneStepKycResponse>
        {
            ErrorCode = "alpha-400",
            ErrorMessage = "invalid payload"
        };
        var response = new ApiResponse<JibitAlphaResponseWrapper<OneStepKycResponse>>(
            HttpStatusCode.BadRequest,
            wrapper,
            rawContent: "{}");

        wrapper.AddError(response);

        Assert.NotNull(wrapper.Errors);
        var error = Assert.Single(wrapper.Errors);
        Assert.Equal("alpha-400", error.Code);
        Assert.Equal("invalid payload", error.Message);
    }

    [Fact]
    public void AddError_WhenWrapperHasNoMessage_ParsesRawContent()
    {
        var wrapper = new JibitAlphaResponseWrapper<OneStepKycResponse>();
        var rawContent = """
        {
          "message": "alpha failed"
        }
        """;
        var response = new ApiResponse<JibitAlphaResponseWrapper<OneStepKycResponse>>(
            HttpStatusCode.InternalServerError,
            wrapper,
            rawContent);

        wrapper.AddError(response);

        Assert.NotNull(wrapper.Errors);
        var error = Assert.Single(wrapper.Errors);
        Assert.Equal(((int)HttpStatusCode.InternalServerError).ToString(), error.Code);
        Assert.Equal("alpha failed", error.Message);
    }

    [Fact]
    public void AddError_WithSuccessStatus_SkipsErrorPopulation()
    {
        var wrapper = new JibitAlphaResponseWrapper<OneStepKycResponse>
        {
            ErrorCode = "alpha-200",
            ErrorMessage = "ok"
        };
        var response = new ApiResponse<JibitAlphaResponseWrapper<OneStepKycResponse>>(
            HttpStatusCode.OK,
            wrapper,
            rawContent: string.Empty);

        wrapper.AddError(response);

        Assert.Null(wrapper.Errors);
    }
}
