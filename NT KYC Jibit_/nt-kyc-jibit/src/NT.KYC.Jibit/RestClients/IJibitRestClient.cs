using NT.KYC.Jibit.RestClients.APIs.AccessToken;
using NT.KYC.Jibit.RestClients.APIs.Cards;
using NT.KYC.Jibit.RestClients.APIs.IdentityDetail;
using NT.KYC.Jibit.RestClients.APIs.Matching;
using NT.KYC.Jibit.Utils;

namespace NT.KYC.Jibit.RestClients;

public interface IJibitRestClient
{
    public JibitRestClientConfiguration JibitConfiguration { get; }
    
    public IJibitAccessTokenApi JibitAccessTokenApi { get; }

    public IIdentityDetailApi IdentityDetailApi { get; }
    
    public IMatchingApi MatchingApi { get; }
    public ICardsApi CardsApi { get; }
    
}