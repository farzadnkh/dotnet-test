using System.Net;
using System.Security.Cryptography.X509Certificates;
using NT.SDK.RestClient.Models;

namespace ExchangeRateProvider.Adapter.Xe.Models.Options
{
    public class XeConfigurations : IReadableConfiguration
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

        public X509CertificateCollection ClientCertificates { get; set; } 

        public void SetApiKey(string apiKey)
        {
            throw new NotImplementedException();
        }
    }
}
