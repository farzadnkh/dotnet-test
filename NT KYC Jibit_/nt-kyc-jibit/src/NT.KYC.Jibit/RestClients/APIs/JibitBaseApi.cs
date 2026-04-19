using Microsoft.Extensions.Logging;
using NT.SDK.RestClient.Clients;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.RestClients.APIs;

public abstract class JibitBaseApi(IReadableConfiguration configuration, ILogger<JibitBaseApi> logger) : ApiClient(configuration, logger);