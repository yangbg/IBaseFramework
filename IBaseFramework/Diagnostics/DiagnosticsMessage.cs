using System;

namespace IBaseFramework.Diagnostics
{
    /// <summary>
    /// 诊断消息
    /// </summary>
    public class DiagnosticsMessage
    {
        /// <summary>
        /// 当前时间戳
        /// </summary>
        public long? Timestamp { get; set; }

        /// <summary>
        /// 操作
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// 操作id
        /// </summary>
        public string OperationId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// sql语句
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// sql参数
        /// </summary>
        public object Parameters { get; set; }

        /// <summary>
        /// 耗时
        /// </summary>
        public long? ElapsedMilliseconds { get; set; }

        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 数据库数据源
        /// </summary>
        public string DataSource { get; set; }
    }
}
