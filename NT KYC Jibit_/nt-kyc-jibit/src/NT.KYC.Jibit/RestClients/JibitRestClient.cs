using NT.KYC.Jibit.RestClients.APIs.AccessToken;
using NT.KYC.Jibit.RestClients.APIs.Cards;
using NT.KYC.Jibit.RestClients.APIs.IdentityDetail;
using NT.KYC.Jibit.RestClients.APIs.Matching;
using NT.KYC.Jibit.Utils;

namespace NT.KYC.Jibit.RestClients;

public class JibitRestClient(
    JibitRestClientConfiguration jibitConfiguration, 
    IJibitAccessTokenApi jibitAccessTokenApi, 
    IIdentityDetailApi identityDetailApi, 
    IMatchingApi matchingApi, 
    ICardsApi cardsApi) : IJibitRestClient
{
    public JibitRestClientConfiguration JibitConfiguration => jibitConfiguration;

    public IJibitAccessTokenApi JibitAccessTokenApi => jibitAccessTokenApi;

    public IIdentityDetailApi IdentityDetailApi => identityDetailApi;

    public IMatchingApi MatchingApi => matchingApi;

    public ICardsApi CardsApi => cardsApi;
}