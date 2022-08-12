namespace Domain.Services.Interfaces
{
    public interface IUrlService
    {
        public string GetBaseServerUrl(string serviceDiscoveryName, string namespaceName);
        public string GetBaseGridUrl(string serviceDiscoveryName, string namespaceName);
    }
}
