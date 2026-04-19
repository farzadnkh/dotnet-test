using Microsoft.Extensions.Hosting;

namespace NT.KYC.Jibit.HashiCorpVault.Extensions;

public static class IHostEnvironmentExtensions
{
    public static bool IsDebugMode(this IHostEnvironment hostEnvironment)
    {
        return hostEnvironment.IsEnvironment("Debug");
    }
}