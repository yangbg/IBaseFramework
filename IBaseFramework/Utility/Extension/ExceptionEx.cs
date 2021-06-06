using System;
using IBaseFramework.Logger.Logging;

namespace IBaseFramework.Utility.Extension
{
    public static class ExceptionEx
    {
        /// <summary>
        /// 写异常信息
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="path"></param>
        public static void WriteTo(this Exception ex)
        {
            var logger = LoggerManager.Logger<Exception>();
            logger.Error(ex.Message, ex);
        }
    }
}