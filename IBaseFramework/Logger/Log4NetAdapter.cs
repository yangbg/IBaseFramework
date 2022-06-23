using System.Configuration;
using System.IO;
using IBaseFramework.Logger.Logging;
using log4net;
using log4net.Config;
using log4net.Repository;
using ILog = IBaseFramework.Logger.Logging.ILog;

namespace IBaseFramework.Logger
{
    public class Log4NetAdapter : LoggerAdapterBase
    {
        private static string _fileName = "log4net.config";
        private static string _configPath = string.Empty;

        /// <summary>
        /// 初始化一个<see cref="Log4NetAdapter"/>类型的新实例
        /// </summary>k
        public Log4NetAdapter(string configFullPathSettingName, string webSite = "")
        {
            _fileName = string.Empty;
            _configPath = ConfigurationManager.AppSettings[configFullPathSettingName];

            AdapterInit(webSite);
        }

        public Log4NetAdapter(string configPath, string configName = "log4net.config", string webSite = "")
        {
            _fileName = configName;
            _configPath = configPath;

            AdapterInit(webSite);
        }
        public static ILoggerRepository Repository { get; set; }
        private void AdapterInit(string webSite)
        {
            var configFile = Path.Combine(_configPath, _fileName);
            if (!File.Exists(configFile))
                return;
            GlobalContext.Properties["WebSite"] = string.IsNullOrEmpty(webSite) ? "logs" : webSite;

            Repository = LogManager.CreateRepository("NETCoreRepository");
            XmlConfigurator.ConfigureAndWatch(Repository, new FileInfo(configFile));


            //            var appender = new RollingFileAppender
            //            {
            //                Name = "root",
            //                File = "logs\\log_",
            //                AppendToFile = true,
            //                LockingModel = new FileAppender.MinimalLock(),
            //                RollingStyle = RollingFileAppender.RollingMode.Date,
            //                DatePattern = "yyyyMMdd-HH\".log\"",
            //                StaticLogFileName = false,
            //                MaxSizeRollBackups = 10,
            //                Layout = new PatternLayout("[%d{yyyy-MM-dd HH:mm:ss.fff}] %-5p %c %t %w %n%m%n")
            //                //Layout = new PatternLayout("[%d [%t] %-5p %c [%x] - %m%n]")
            //            };
            //            appender.ClearFilters();
            //            appender.AddFilter(new LevelRangeFilter
            //            {
            //                LevelMin = Level.Debug,
            //                LevelMax = Level.Fatal
            //            });
            //            appender.ActivateOptions();
            //            BasicConfigurator.Configure(appender);
        }

        protected override ILog CreateLogger(string name)
        {
            var log = LogManager.GetLogger(Repository.Name, name);
            return new Log4NetLog(log);
        }
    }
}
