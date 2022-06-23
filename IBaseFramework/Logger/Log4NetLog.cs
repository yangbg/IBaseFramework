using System;
using System.Diagnostics;
using System.Text;
using IBaseFramework.Extension;
using IBaseFramework.Logger.Logging;
using log4net.Core;
using ILogger = log4net.Core.ILogger;
using LoggerManager = IBaseFramework.Logger.Logging.LoggerManager;


namespace IBaseFramework.Logger
{
    public class Log4NetLog : LogBase
    {
        private readonly ILogger _logger;

        public Log4NetLog(ILoggerWrapper wrapper)
        {
            _logger = wrapper.Logger;
        }

        private static StackFrame CurrentStrace()
        {
            var strace = new StackTrace(true);
            for (var i = 0; i < strace.FrameCount; i++)
            {
                var frame = strace.GetFrame(i);
                if (string.IsNullOrWhiteSpace(frame.GetFileName()))
                    continue;
                var method = frame.GetMethod();
                if (method.Name == "OnException" || method.DeclaringType == typeof(LogBase) ||
                    method.DeclaringType == (typeof(LoggerManager)) ||
                    method.DeclaringType == typeof(Log4NetLog))
                    continue;
                return frame;
            }
            return null;
        }

        private static LogInfo Format(object msgObject, Exception ex = null)
        {
            var frame = CurrentStrace();
            string method = string.Empty, fileInfo = string.Empty;
            if (frame != null)
            {
                method = string.Concat(frame.GetMethod().DeclaringType, " - ", frame.GetMethod().Name);
                fileInfo = $"{frame.GetFileName()}[{frame.GetFileLineNumber()}]";
            }
            if (ex != null && string.IsNullOrWhiteSpace(method))
            {
                method = $"{ex.TargetSite.DeclaringType} - {ex.TargetSite.Name}";
            }
            string msg;
            if (msgObject == null || msgObject is string)
            {
                msg = (msgObject ?? string.Empty).ToString();
            }
            else
            {
                msg = msgObject.ToJson();
            }
            var result = new LogInfo
            {
                Method = method,
                File = fileInfo,
                Message = msg,
                Detail = string.Empty
            };
            if (ex != null)
            {
                result.Detail = FormatEx(ex);
            }
            return result;
        }

        protected override void WriteInternal(LogLevel level, object message, Exception exception)
        {
            _logger.Log(typeof(Log4NetLog), ParseLevel(level), Format(message, exception),
                exception);
        }

        public override bool IsTraceEnabled => _logger.IsEnabledFor(Level.Trace) || _logger.IsEnabledFor(Level.All);

        public override bool IsDebugEnabled => _logger.IsEnabledFor(Level.Debug) || _logger.IsEnabledFor(Level.All);

        public override bool IsInfoEnabled => _logger.IsEnabledFor(Level.Info) || _logger.IsEnabledFor(Level.All);

        public override bool IsWarnEnabled => _logger.IsEnabledFor(Level.Warn) || _logger.IsEnabledFor(Level.All);

        public override bool IsErrorEnabled => _logger.IsEnabledFor(Level.Error) || _logger.IsEnabledFor(Level.All);

        public override bool IsFatalEnabled => _logger.IsEnabledFor(Level.Fatal) || _logger.IsEnabledFor(Level.All);

        private Level ParseLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.All:
                    return Level.All;
                case LogLevel.Trace:
                    return Level.Trace;
                case LogLevel.Debug:
                    return Level.Debug;
                case LogLevel.Info:
                    return Level.Info;
                case LogLevel.Warn:
                    return Level.Warn;
                case LogLevel.Error:
                    return Level.Error;
                case LogLevel.Fatal:
                    return Level.Fatal;
                case LogLevel.Off:
                    return Level.Off;
                default:
                    return Level.Off;
            }
        }

        #region 异常信息格式化
        /// <summary>
        /// 异常信息格式化
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="isHideStackTrace"></param>
        /// <returns></returns>
        private static string FormatEx(Exception ex, bool isHideStackTrace = false)
        {
            var sb = new StringBuilder();
            var count = 0;
            var appString = string.Empty;
            while (ex != null)
            {
                if (count > 0)
                {
                    appString += "  ";
                }
                sb.AppendLine($"{appString}异常消息：{ex.Message}");
                sb.AppendLine($"{appString}异常类型：{ex.GetType().FullName}");
                sb.AppendLine($"{appString}异常方法：{(ex.TargetSite == null ? null : ex.TargetSite.Name)}");
                sb.AppendLine($"{appString}异常源：{ex.Source}");
                if (!isHideStackTrace && ex.StackTrace != null)
                {
                    sb.AppendLine($"{appString}异常堆栈：{ex.StackTrace}");
                }
                if (ex.InnerException != null)
                {
                    sb.AppendLine($"{appString}内部异常：");
                    count++;
                }
                ex = ex.InnerException;
            }
            return sb.ToString();
        }
        #endregion
    }
}
