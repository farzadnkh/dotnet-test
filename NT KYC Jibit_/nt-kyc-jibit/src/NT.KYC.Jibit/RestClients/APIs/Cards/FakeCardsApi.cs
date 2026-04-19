using Microsoft.Extensions.Logging;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;
using System.Net;

namespace NT.KYC.Jibit.RestClients.APIs.Cards;

public class FakeCardsApi(JibitRestClientConfiguration configuration, ILogger<FakeCardsApi> logger) : JibitBaseApi(configuration, logger), ICardsApi
{
    public async Task<ApiResponse<JibitResponseWrapper<CardNumberToIbanResponse>>> CardNumberToIban(CardNumberToIbanRequest request, string bearerToken)
    {
        await Task.Delay(10);

        var response = new CardNumberToIbanResponse
        {
            Number = "654"
        };

        var wrapper = new JibitResponseWrapper<CardNumberToIbanResponse>
        {
            Data = response
        };

        return new ApiResponse<JibitResponseWrapper<CardNumberToIbanResponse>>(HttpStatusCode.OK, wrapper);
    }
}