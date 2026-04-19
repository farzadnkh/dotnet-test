using Microsoft.Extensions.Logging;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.RestClients.APIs.Cards;

public interface ICardsApi
{
    /// <summary>
    /// This Endpoint return the Iban number of the bank account owner.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    public Task<ApiResponse<JibitResponseWrapper<CardNumberToIbanResponse>>> CardNumberToIban(
        CardNumberToIbanRequest request, string bearerToken);
}

public class CardsApi(JibitRestClientConfiguration configuration, ILogger<CardsApi> logger) : JibitBaseApi(configuration, logger), ICardsApi
{
    public async Task<ApiResponse<JibitResponseWrapper<CardNumberToIbanResponse>>> CardNumberToIban(CardNumberToIbanRequest request, string bearerToken)
    {
        var options = RequestOptions<CardNumberToIbanRequest>.CreateDefault();

        options.SetBody(request);

        options.HeaderParameters.Add("Authorization", "Bearer " + bearerToken);

        options.QueryParameters.Add(CardsApiConstants.Iban, request.Iban.ToString());
        options.QueryParameters.Add(CardsApiConstants.CardNumber, request.CardNumber);

        var result =
            await GetAsync<JibitResponseWrapper<CardNumberToIbanResponse>>(JibitEndPoints.CardsService,
                options);

        result.Result.AddJibitErrorResponse(result);
        if (result.Successed) result.Result.TransferToObject(result.RawContent);
        
        return result;
    }
}