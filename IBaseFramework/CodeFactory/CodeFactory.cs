using System.Collections.Generic;

namespace IBaseFramework.CodeFactory
{
    public partial class CodeFactory
    {
        /// <summary>
        /// 公共字段
        /// </summary>
        internal static List<string> CommonColumns = new List<string>()
        {
            "IsDeleted".ToLower(),
            "Version".ToLower(),
            "CreateUserId".ToLower(),
            "CreateTime".ToLower(),
            "UpdateUserId".ToLower(),
            "UpdateTime".ToLower()
        };
    }
}