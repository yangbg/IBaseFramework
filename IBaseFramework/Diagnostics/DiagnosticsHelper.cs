using IBaseFramework.ExpressionVisit;
using IBaseFramework.Infrastructure;
using System;
using System.Data;
using System.Diagnostics;

namespace IBaseFramework.Diagnostics
{
    public class DiagnosticsHelper
    {
        /// <summary>
        /// 诊断日志
        /// </summary>

        private static readonly DiagnosticSource _diagnosticListener = new Lazy<DiagnosticListener>(() =>
        {
            var listener = new DiagnosticListener(DiagnosticStrings.DiagnosticListenerName);

            if (DapperContext.DatabaseConfiguration.IsOpenSqlDiagnostics || DapperContext.DatabaseConfiguration.OnSqlDiagnostics != null)
            {
                listener.SubscribeWithAdapter(new CustomListener());
            }

            return listener;
        }).Value;

        /// <summary>
        /// 执行前诊断
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">sql参数</param>
        /// <param name="dataSource">数据源</param>
        /// <returns></returns>
        public static DiagnosticsMessage ExecuteBefore(SqlQuery sqlQuery, IDbConnection dbConnection)
        {
            if (!_diagnosticListener.IsEnabled(DiagnosticStrings.BeforeExecute))
                return null;

            var message = new DiagnosticsMessage
            {
                Sql = sqlQuery.GetSql(),
                Parameters = sqlQuery.Params,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Operation = DiagnosticStrings.BeforeExecute,
            };

            _diagnosticListener.Write(DiagnosticStrings.BeforeExecute, new { DiagnosticsMessage = message });

            return message;
        }


        /// <summary>
        /// 执行前诊断
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">sql参数</param>
        /// <param name="dataSource">数据源</param>
        /// <returns></returns>
        public static DiagnosticsMessage ExecuteBefore(string sql, object paras, IDbConnection dbConnection)
        {
            if (!_diagnosticListener.IsEnabled(DiagnosticStrings.BeforeExecute))
                return null;

            var message = new DiagnosticsMessage
            {
                Sql = sql,
                Parameters = paras,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Operation = DiagnosticStrings.BeforeExecute,
            };

            _diagnosticListener.Write(DiagnosticStrings.BeforeExecute, new { DiagnosticsMessage = message });

            return message;
        }

        /// <summary>
        /// 执行后诊断
        /// </summary>
        /// <param name="message">诊断消息</param>
        /// <returns></returns>
        public static void ExecuteAfter(DiagnosticsMessage message)
        {
            if (!_diagnosticListener.IsEnabled(DiagnosticStrings.AfterExecute))
                return;

            if (message?.Timestamp != null)
            {
                message.Sql = null;
                message.Parameters = null;  
                message.Operation = DiagnosticStrings.AfterExecute;
                message.ElapsedMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - message.Timestamp.Value;

                _diagnosticListener.Write(DiagnosticStrings.AfterExecute, new { DiagnosticsMessage = message });
            }
        }

        /// <summary>
        /// 执行异常诊断
        /// </summary>
        /// <param name="message">诊断消息</param>
        /// <param name="exception">异常</param>
        public static void ExecuteError(Exception exception)
        {
            if (!_diagnosticListener.IsEnabled(DiagnosticStrings.ErrorExecute))
                return;

            _diagnosticListener.Write(DiagnosticStrings.ErrorExecute, new { Operation = DiagnosticStrings.ErrorExecute, Exception = exception, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
        }

        /// <summary>
        /// 执行释放诊断
        /// </summary>
        /// <param name="message">诊断消息</param>
        public static void ExecuteDispose(IDbConnection dbConnection)
        {
            if (!_diagnosticListener.IsEnabled(DiagnosticStrings.DisposeExecute))
                return;

            _diagnosticListener.Write(DiagnosticStrings.DisposeExecute, new { DataSource = dbConnection.ConnectionString, Operation = DiagnosticStrings.DisposeExecute, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
        }
    }
}
