using IBaseFramework.Extension;
using IBaseFramework.Infrastructure;
using IBaseFramework.Logger.Logging;
using Microsoft.Extensions.DiagnosticAdapter;
using System;

namespace IBaseFramework.Diagnostics
{
    public class CustomListener
    {
        /// <summary>
        /// 执行前诊断
        /// </summary>
        [DiagnosticName(DiagnosticStrings.BeforeExecute)]
        public void ExecuteBefore(DiagnosticsMessage diagnosticsMessage)
        {
            if (DapperContext.DatabaseConfiguration.OnSqlDiagnostics != null)
            {
                DapperContext.DatabaseConfiguration.OnSqlDiagnostics.Invoke(diagnosticsMessage);
            }
            else
            {
                var logger = LoggerManager.Logger<CustomListener>();
                logger?.Info(diagnosticsMessage.ToJson());
            }
        }

        /// <summary>
        /// 执行后诊断
        /// </summary>
        [DiagnosticName(DiagnosticStrings.AfterExecute)]
        public void ExecuteAfter(DiagnosticsMessage diagnosticsMessage)
        {
            if (DapperContext.DatabaseConfiguration.OnSqlDiagnostics != null)
            {
                DapperContext.DatabaseConfiguration.OnSqlDiagnostics.Invoke(diagnosticsMessage);
            }
            else
            {
                var logger = LoggerManager.Logger<CustomListener>();
                logger?.Info(diagnosticsMessage.ToJson());
            }
        }

        /// <summary>
        /// 执行异常诊断
        /// </summary>
        [DiagnosticName(DiagnosticStrings.ErrorExecute)]
        public void ExecuteError(string operation, Exception exception, long timestamp)
        {
            var msg = new DiagnosticsMessage { Operation = operation, Timestamp = timestamp, Exception = exception };
            if (DapperContext.DatabaseConfiguration.OnSqlDiagnostics != null)
            {
                DapperContext.DatabaseConfiguration.OnSqlDiagnostics.Invoke(msg);
            }
            else
            {
                var logger = LoggerManager.Logger<CustomListener>();
                logger?.Info(msg.ToJson());
            }
        }

        /// <summary>
        /// 执行数据库连接释放
        /// </summary>
        [DiagnosticName(DiagnosticStrings.DisposeExecute)]
        public void ExecuteDispose(string dataSource, string operation, long timestamp)
        {
            var msg = new DiagnosticsMessage { Operation = operation, Timestamp = timestamp, DataSource = dataSource };
            if (DapperContext.DatabaseConfiguration.OnSqlDiagnostics != null)
            {
                DapperContext.DatabaseConfiguration.OnSqlDiagnostics.Invoke(msg);
            }
            else
            {
                var logger = LoggerManager.Logger<CustomListener>();
                logger?.Info(msg.ToJson());
            }
        }

    }
}
