using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace IBaseFramework.CodeFactory
{
    /// <summary>
    /// 实体生成工厂
    /// </summary>
    public partial class CodeFactory
    {
        #region 创建Entity
        /// <summary>
        /// 创建Entity
        /// </summary>
        /// <param name="connectionStr">连接字符串</param>
        /// <param name="dataBase">数据库</param>
        /// <param name="nameSpacePrefix">命名空间前缀</param>
        /// <param name="projectPath">生成到项目目录</param>
        /// <returns></returns>
        public static void MakeEntity(string connectionStr, string dataBase, string nameSpacePrefix, string projectPath)
        {
            if (string.IsNullOrEmpty(connectionStr))
                throw new Exception("connectionStr can not be empty!");

            if (string.IsNullOrEmpty(dataBase))
                throw new Exception("dataBase can not be empty!");

            if (string.IsNullOrEmpty(projectPath))
                throw new Exception("projectPath can not be empty!");

            var tableDetails = GetTableDetails(connectionStr, dataBase);
            if (tableDetails == null || tableDetails.Count < 1)
                throw new Exception("There are no tables!");

            foreach (var detail in tableDetails)
            {
                var className = detail.TableName.Split('_').Last();

                var sb = new StringBuilder();

                sb.AppendLine("using IBaseFramework.Base;");
                sb.AppendLine("using IBaseFramework.DapperExtension;");
                sb.AppendLine("using System.ComponentModel;");

                if (!string.IsNullOrEmpty(nameSpacePrefix))
                {
                    sb.AppendLine($"namespace {nameSpacePrefix}.Entity");
                    sb.AppendLine("{");
                }

                sb.AppendLine($"\t[Table(\"{detail.TableName}\")]");
                sb.AppendLine($"\t[Description(\"{detail.TableComment}\")]");
                sb.AppendLine($"\tpublic class {className} : EntityBase");
                sb.AppendLine("\t{");
                sb.AppendLine("\t\tpublic static class M");
                sb.AppendLine("\t\t{");
                sb.AppendLine($"\t\t\tpublic const string TableName = \"{detail.TableName}\";");

                foreach (var field in detail.TableFields)
                {
                    sb.AppendLine($"\t\t\tpublic const string {field.ColumnName} = \"{field.ColumnName}\";");
                }

                sb.AppendLine("\t\t}");
                sb.AppendLine("");

                foreach (var field in detail.TableFields)
                {
                    if (CommonColumns.Contains(field.ColumnName.ToLower()))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(field.Comment))
                    {
                        sb.AppendLine("\t\t/// <summary>");
                        sb.AppendLine($"\t\t/// {field.Comment}");
                        sb.AppendLine("\t\t/// </summary>");
                        sb.AppendLine($"\t\t[Description(\"{field.Comment}\")]");
                    }
                    if (string.Equals(field.ColumnKey, "PRI", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sb.AppendLine("\t\t[Key]");
                    }
                    sb.AppendLine($"\t\tpublic {field.DataType} {field.ColumnName} {{ get; set; }}\n");
                }

                sb.AppendLine("\t}");

                if (!string.IsNullOrEmpty(nameSpacePrefix))
                {
                    sb.AppendLine("}");
                }

                var filePath = Path.Combine(projectPath, $"{className}.cs");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
            }

        }
        #endregion

        #region 获取GetTableDetails
        /// <summary>
        /// 获取GetTableDetails
        /// </summary>
        /// <param name="connectionStr">连接字符串</param>
        /// <param name="dataBase">数据库</param>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        public static List<TableDetail> GetTableDetails(string connectionStr, string dataBase, string tableName = "")
        {
            var result = new List<TableDetail>();

            if (string.IsNullOrEmpty(tableName))
            {
                var getTables = $@"select table_name,TABLE_COMMENT from information_schema.tables where table_schema='{dataBase}'";
                var tables = GetTable(connectionStr, getTables);
                if (tables != null)
                {
                    for (var index = 0; index < tables.Rows.Count; index++)
                    {
                        var tableRow = tables.Rows[index];
                        tableName = tableRow["table_name"].ToString();

                        var tableDetail = new TableDetail()
                        {
                            TableName = tableName,
                            TableComment = tableRow["TABLE_COMMENT"].ToString(),
                            TableFields = GetFields(connectionStr, dataBase, tableName)
                        };

                        result.Add(tableDetail);
                    }
                }
            }
            else
            {
                result.Add(new TableDetail()
                {
                    TableName = tableName,
                    TableFields = GetFields(connectionStr, dataBase, tableName)
                });
            }

            return result;
        }
        #endregion

        #region Private
        private static List<TableField> GetFields(string connectionStr, string dataBase, string tableName)
        {
            var getFields = $@"select column_name AS ColumnName,data_type AS DataType,COLUMN_TYPE AS ColumnType,column_comment AS Comment,extra AS Extra,CHARACTER_MAXIMUM_LENGTH AS MaxLength,
            IS_NULLABLE AS IsNullable,COLUMN_KEY AS ColumnKey from INFORMATION_SCHEMA.COLUMNS Where table_name = '{tableName}' and table_schema = '{dataBase}'";

            var fields = GetTable(connectionStr, getFields);
            if (fields == null)
                return null;

            var result = new List<TableField>();
            for (var index = 0; index < fields.Rows.Count; index++)
            {
                var fieldsRow = fields.Rows[index];
                result.Add(new TableField()
                {
                    ColumnName = fieldsRow["ColumnName"].ToString(),
                    ColumnKey = fieldsRow["ColumnKey"].ToString(),
                    ColumnType = fieldsRow["ColumnType"].ToString(),
                    Comment = Regex.Replace(fieldsRow["Comment"].ToString(), @"\r\n?|\n|\t", "").Trim(),
                    DataType = GetFiledType(fieldsRow["DataType"].ToString(), string.Equals(fieldsRow["IsNullable"].ToString(), "Yes", StringComparison.CurrentCultureIgnoreCase)),
                    Extra = fieldsRow["Extra"].ToString(),
                    IsNullable = fieldsRow["IsNullable"].ToString(),
                    MaxLength = fieldsRow["MaxLength"].ToString()
                });
            }

            return result;

        }

        private static DataTable GetTable(string connectionStr, string sql)
        {
            var ds = MySqlHelper.ExecuteDataset(connectionStr, sql);
            return ds?.Tables[0];
        }

        private static string GetFiledType(string type, bool isNullable)
        {
            var isNullableStr = isNullable ? "?" : "";

            switch (type)
            {
                case "tinyint":
                    return "byte" + isNullableStr;
                case "smallint":
                    return "short" + isNullableStr;
                case "double":
                    return "double" + isNullableStr;
                case "decimal":
                    return "decimal" + isNullableStr;
                case "float":
                    return "float" + isNullableStr;
                case "bit":
                    return "bool" + isNullableStr;
                case "int":
                    return "int" + isNullableStr;
                case "bigint":
                    return "long" + isNullableStr;
                case "varchar":
                case "char":
                    return "string";
                case "nvarchar":
                    return "string";
                case "datetime":
                case "timestamp":
                case "date":
                    return "System.DateTime" + isNullableStr;
                case "text":
                    return "string";
                default:
                    return type;
            }
        }
        #endregion
    }

    public class TableDetail
    {
        public string TableName { get; set; }
        public string TableComment { get; set; }
        public List<TableField> TableFields { get; set; }
    }

    public class TableField
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string ColumnType { get; set; }
        public string Comment { get; set; }
        public string Extra { get; set; }
        public string MaxLength { get; set; }
        public string IsNullable { get; set; }
        public string ColumnKey { get; set; }
    }
}