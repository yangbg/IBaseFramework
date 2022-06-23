using System;
using IBaseFramework.Base;
using IBaseFramework.Diagnostics;
using IBaseFramework.Ioc;

namespace IBaseFramework.Infrastructure
{
    public class DapperContext
    {
        public static event Action<DapperContextConfig> DbConfigEventHandle;
        private static DapperContextConfig _config = null;

        /// <summary>
        /// 配置
        /// </summary>
        internal static DapperContextConfig DatabaseConfiguration
        {
            get
            {
                if (_config != null)
                    return _config;

                if (DbConfigEventHandle == null)
                    throw new Exception("Please register database configuration event first!");

                _config = new DapperContextConfig();
                DbConfigEventHandle.Invoke(_config);

                if (string.IsNullOrEmpty(_config.ConnectionString))
                    throw new Exception("ConnectionString can not be null!");

                return _config;
            }
        }

        internal static object GetUserId()
        {
            if (!DatabaseConfiguration.IsRegisterUserId)
                return default;

            var userManagerProvider = IocManager.Resolve<IUserManagerProvider>();
            if (userManagerProvider == null)
            {
                throw new Exception("Please implement the interface IUserManagerProvider<TKey>!");
            }

            return userManagerProvider.GetUserId();
        }
    }

    public class DapperContextConfig
    {
        /// <summary>
        ///     ConnectionString
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Type Sql provider
        /// </summary>
        public SqlProvider SqlProvider { get; set; } = SqlProvider.MySQL;

        /// <summary>
        ///     Register UserId(implement the interface IUserManagerProvider)
        /// </summary>
        public bool IsRegisterUserId { get; set; } = false;

        /// <summary>
        ///     Is Open SqlDiagnostics
        /// </summary>
        public bool IsOpenSqlDiagnostics { get; set; } = false;

        /// <summary>
        /// Sql Diagnostics
        /// </summary>
        public Action<DiagnosticsMessage> OnSqlDiagnostics { get; set; }
    }
}