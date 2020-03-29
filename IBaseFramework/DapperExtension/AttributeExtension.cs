using System;

namespace IBaseFramework.DapperExtension
{
    /// <summary>
    /// 表名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string tableName)
        {
            Name = tableName;
        }

        public string Name { get; set; }
    }

    /// <summary>
    /// 自增主键
    /// </summary>
    // do not want to depend on data annotations that is not in client profile
    [AttributeUsage(AttributeTargets.Property)]
    public class AutoIncrementKeyAttribute : Attribute
    {
    }

    /// <summary>
    /// 非自增主键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
    }

    /// <summary>
    /// 忽略
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
    }
}
