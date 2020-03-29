using System;
using IBaseFramework.DapperExtension;

namespace IBaseFramework.Infrastructure
{
    public class DapperContext
    {
        public static event Action<DapperContextConfig> DatabaseConfigurationEventHandle;

        private static DapperContextConfig _dapperContextConfig = null;

        /// <summary>
        /// 配置
        /// </summary>
        internal static DapperContextConfig DatabaseConfiguration
        {
            get
            {
                if (_dapperContextConfig != null)
                    return _dapperContextConfig;

                if (DatabaseConfigurationEventHandle == null)
                    throw new Exception("Please register database configuration event first!");

                _dapperContextConfig = new DapperContextConfig();
                DatabaseConfigurationEventHandle.Invoke(_dapperContextConfig);

                if (string.IsNullOrEmpty(_dapperContextConfig.ConnectionString))
                    throw new Exception("ConnectionString can not be null!");

                return _dapperContextConfig;
            }
        }
    }
}