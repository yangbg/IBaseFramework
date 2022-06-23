namespace IBaseFramework.Diagnostics
{
    /// <summary>
    /// 诊断常量
    /// </summary>
    public static class DiagnosticStrings
    {
        /// <summary>
        /// 监听名称
        /// </summary>
        internal const string DiagnosticListenerName = "IBaseFrameworkDiagnosticsListener";

        /// <summary>
        /// 前缀
        /// </summary>
        internal const string Prefix = "IBaseFramework_";

        /// <summary>
        /// 执行前
        /// </summary>
        public const string BeforeExecute = Prefix + "ExecuteBefore";

        /// <summary>
        /// 执行后
        /// </summary>
        public const string AfterExecute = Prefix + "ExecuteAfter";

        /// <summary>
        /// 执行异常
        /// </summary>
        public const string ErrorExecute = Prefix + "ExecuteError";

        /// <summary>
        /// 执行数据库连接释放
        /// </summary>
        public const string DisposeExecute = Prefix + "ExecuteDispose";
    }
}
