using IBaseFramework.Base;
using Microsoft.Extensions.Configuration;

namespace IBaseFramework.Helper
{
    public interface IConfigHelper : IDependency
    {
        IConfigurationSection GetSection(string key);
        string GetConfigurationValue(string key);
        string GetConfigurationValue(string section, string key);
        string GetConnectionString(string key);
    }

    public class ConfigHelper : IConfigHelper
    {
        private readonly IConfiguration _configuration;
        public ConfigHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfigurationSection GetSection(string key)
        {
            return _configuration.GetSection(key);
        }

        public string GetConfigurationValue(string key)
        {
            return _configuration[key];
        }

        public string GetConfigurationValue(string section, string key)
        {
            return GetSection(section)?[key];
        }

        public string GetConnectionString(string key)
        {
            return _configuration.GetConnectionString(key);
        }
    }
}