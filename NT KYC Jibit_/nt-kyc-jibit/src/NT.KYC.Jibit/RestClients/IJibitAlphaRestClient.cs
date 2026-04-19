using NT.KYC.Jibit.RestClients.APIs.BiometricKyc;
using NT.KYC.Jibit.RestClients.APIs.DocumentKyc;
using NT.KYC.Jibit.Utils;

namespace NT.KYC.Jibit.RestClients;

public interface IJibitAlphaRestClient
{
    public JibitAlphaRestClientConfiguration JibitAlphaConfiguration { get; }
    public IBiometricKycApi BiometricKycApi { get; }
    public IDocumentKycApi DocumentKycApi { get; }
}