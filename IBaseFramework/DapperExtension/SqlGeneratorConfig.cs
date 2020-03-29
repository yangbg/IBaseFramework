using System;
using System.Collections.Generic;
using System.Linq;
using IBaseFramework.Base;

namespace IBaseFramework.DapperExtension
{
    /// <summary>
    ///     Config for SqlGenerator
    /// </summary>
    public class SqlGeneratorConfig
    {
        public SqlGeneratorConfig(SqlProvider sqlProvider)
        {
            StartQuotationMark = string.Empty;
            EndQuotationMark = string.Empty;

            switch (sqlProvider)
            {
                case SqlProvider.MSSQL:
                    StartQuotationMark = "[";
                    EndQuotationMark = "]";

                    break;

                case SqlProvider.MySQL:
                    StartQuotationMark = "`";
                    EndQuotationMark = "`";

                    break;

                case SqlProvider.PostgreSQL:
                    StartQuotationMark = "\"";
                    EndQuotationMark = "\"";

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(SqlProvider));
            }

            SqlProvider = sqlProvider;
        }

        public string StartQuotationMark
        {
            get;
        }

        public string EndQuotationMark
        {
            get;
        }

        /// <summary>
        ///     Type Sql provider
        /// </summary>
        public SqlProvider SqlProvider
        {
            get;
        }

        /// <summary>
        ///     LogicDelete String
        /// </summary>
        public string LogicDeleteSql => " IsDeleted=1 ";

        /// <summary>
        /// 实体公共属性
        /// </summary>
        public List<string> CommonProperty => typeof(EntityBase).GetProperties().Select(u => u.Name).ToList();

        /// <summary>
        /// 更新公共字段
        /// </summary>
        public Dictionary<string, object> UpdateCommonProperty(object userId)
        {
            var now = DateTime.Now;
            return new Dictionary<string, object>()
            {
                {
                    "UpdateUserId",
                    userId
                },
                {
                    "UpdateTime",
                    now
                },
                {
                    "Version",
                    now
                }
            };
        }

    }
}