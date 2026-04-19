using NT.KYC.Jibit.RestClients.APIs.BiometricKyc;
using NT.KYC.Jibit.RestClients.APIs.DocumentKyc;
using NT.KYC.Jibit.Utils;

namespace NT.KYC.Jibit.RestClients;

public class JibitAlphaRestClient(JibitAlphaRestClientConfiguration jibitAlphaConfiguration, IBiometricKycApi biometricKycApi, IDocumentKycApi documentKycApi) : IJibitAlphaRestClient
{
    public JibitAlphaRestClientConfiguration JibitAlphaConfiguration => jibitAlphaConfiguration;

    public IBiometricKycApi BiometricKycApi => biometricKycApi;

    public IDocumentKycApi DocumentKycApi => documentKycApi;
}