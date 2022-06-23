using System;
using IBaseFramework.Logger.Logging;

namespace IBaseFramework.Extension
{
    public static class ExceptionEx
    {
        /// <summary>
        /// 写异常信息
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="msg"></param>
        public static void WriteToFile(this Exception ex, string msg = "")
        {
            var logger = LoggerManager.Logger<Exception>();
            logger.Error(msg + ":" + ex.Message, ex);
        }
    }
}