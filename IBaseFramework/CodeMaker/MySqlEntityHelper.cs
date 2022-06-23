using IBaseFramework.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace IBaseFramework.CodeMaker
{
    public class MySqlEntityHelper
    {
        public static void MakeTableDetailsToJsonFile(string connectionStr, string dataBase, string jsonSavePath)
        {
            var tableDetails = GetTableDetails(connectionStr, dataBase);

            var jsonFilePath = Path.Combine(jsonSavePath, "db_tables.json");
            if (File.Exists(jsonFilePath))
            {
                File.Delete(jsonFilePath);
            }

            File.AppendAllText(jsonFilePath, tableDetails.ToJson(), Encoding.UTF8);
        }

        public static List<TableDetail> GetTableDetails(string connectionStr, string dataBase)
        {
            var result = new List<TableDetail>();

            var getTables = $@"select table_name,TABLE_COMMENT from information_schema.tables where table_schema='{dataBase}'";
            var tables = GetTable(connectionStr, getTables);
            if (tables != null)
            {
                for (var index = 0; index < tables.Rows.Count; index++)
                {
                    var tableRow = tables.Rows[index];
                    var tableName = tableRow["table_name"].ToString();

                    var tableDetail = new TableDetail()
                    {
                        TableName = tableName,
                        TableComment = tableRow["TABLE_COMMENT"].ToString(),
                        TableFields = GetFields(connectionStr, dataBase, tableName)
                    };

                    result.Add(tableDetail);
                }
            }

            return result;
        }

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
                    MaxLength = fieldsRow["MaxLength"].ToString(),
                    IsPrimaryKey = string.Equals(fieldsRow["ColumnKey"].ToString(), "PRI", StringComparison.CurrentCultureIgnoreCase),
                    IsAutoIncrement = string.Equals(fieldsRow["Extra"].ToString(), "auto_increment", StringComparison.CurrentCultureIgnoreCase)
                });
            }

            return result;

        }

        private static DataTable GetTable(string connectionStr, string sql)
        {
            var ds = MySql.Data.MySqlClient.MySqlHelper.ExecuteDataset(connectionStr, sql);
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
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
    }
}
