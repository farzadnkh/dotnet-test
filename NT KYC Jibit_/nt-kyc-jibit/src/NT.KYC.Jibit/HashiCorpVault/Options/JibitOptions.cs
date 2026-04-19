using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using NT.SDK.RestClient.Logger;

namespace NT.KYC.Jibit.HashiCorpVault.Options;

public class JibitOptions
{
    public WebProxy? Proxy { get; set; }

    public bool IsDebugMode { get; set; } = false;

    public ServiceLifetime JibitRestClientLifeCycle { get; set; } = ServiceLifetime.Transient;

    public ServiceLifetime JibitAlphaRestClientLifeCycle { get; set; } = ServiceLifetime.Transient;

    public Action<ApiLogger> Logger { get; set; } = null!;

    public string? TempFolderPath { get; set; }

    public int Timeout { get; set; }

    public IDictionary<string, string>? DefaultHeaders { get; set; }

    public string JibitBasePath { get; set; } = string.Empty;

    public string JibitAlphaBasePath { get; set; } = string.Empty;

    public string? DateTimeFormat { get; set; }

    public X509CertificateCollection? ClientCertificates { get; set; } = null!;
}