using System;

namespace IBaseFramework.Infrastructure
{
    public class DapperContext
    {
        public static event Action<DapperContextConfig> DatabaseConfigurationEventHandle;

        /// <summary>
        /// 配置
        /// </summary>
        internal static DapperContextConfig DatabaseConfiguration
        {
            get
            {
                if (DatabaseConfigurationEventHandle == null)
                    throw new Exception("Please register database configuration event first!");

                var dapperContextConfig = new DapperContextConfig();
                DatabaseConfigurationEventHandle.Invoke(dapperContextConfig);

                if (string.IsNullOrEmpty(dapperContextConfig.ConnectionString))
                    throw new Exception("ConnectionString can not be null!");

                return dapperContextConfig;
            }
        }
    }
}