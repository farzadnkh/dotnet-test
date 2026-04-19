using NT.SDK.RestClient.Logger;
using NT.SDK.RestClient.Models;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace ExchangeRateProvider.Adapter.CryptoCompare.Models.Options
{
    public class CryptoCompareConfigurations : IReadableConfiguration
    {
        public string ApiKey { get; set; }

        public string AccessToken { get; set; }

        public string BasePath { get; set; }

        public string DateTimeFormat { get; set; }

        public IDictionary<string, string> DefaultHeaders { get; set; }

        public string TempFolderPath { get; set; }

        public int Timeout { get; set; }

        public WebProxy Proxy { get; set; }

        public string UserAgent { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public Action<ApiLogger> Logger { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public X509CertificateCollection ClientCertificates => throw new NotImplementedException();

        public void SetApiKey(string apiKey)
        {
            throw new NotImplementedException();
        }
    }
}
