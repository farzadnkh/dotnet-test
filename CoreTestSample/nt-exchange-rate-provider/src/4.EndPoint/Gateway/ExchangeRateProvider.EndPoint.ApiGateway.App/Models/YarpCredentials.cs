using Yarp.ReverseProxy.Configuration;

namespace ExchangeRateProvider.EndPoint.ApiGateway.App.Models
{
    public class YarpCredentials
    {
        public List<RouteConfig> Routes { get; set; }
        public List<ClusterConfig> Clusters { get; set; }
    }
}
