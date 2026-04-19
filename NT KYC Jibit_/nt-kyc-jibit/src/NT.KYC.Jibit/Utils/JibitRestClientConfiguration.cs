using System.Net;
using System.Security.Cryptography.X509Certificates;
using NT.SDK.RestClient.Logger;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Utils;

public class JibitRestClientConfiguration(
    string apiKey,
    string secretKey,
    string basePath,
    int timeout,
    Action<ApiLogger> logger)
    : IReadableConfiguration
{
    public void SetApiKey(string apiKey)
    {
        ApiKey = apiKey;
    }

    public string ApiKey { get; set; } = apiKey;

    public string? AccessToken { get; set; } = string.Empty;
    public string BasePath { get; } = basePath;

    public string? DateTimeFormat { get; set; }

    public IDictionary<string, string>? DefaultHeaders { get; set; }

    public string? TempFolderPath { get; set; }

    public int Timeout { get; set; } = timeout;

    public WebProxy? Proxy { get; set; }

    public string? UserAgent { get; set; }

    public string? Username { get; set; }

    public string Password { get; } = secretKey;

    public bool IsDebugMode { get; set; } = false;
    public X509CertificateCollection? ClientCertificates { get; set; }

    public Action<ApiLogger> Logger { get; set; } = logger;
}