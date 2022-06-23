namespace IBaseFramework.Infrastructure
{
    public enum SqlProvider
    {
        /// <summary>
        ///     MSSQL
        /// </summary>
        SQLServer,

        /// <summary>
        ///     MySQL
        /// </summary>
        MySQL,

        /// <summary>
        ///     PostgreSQL
        /// </summary>
        PostgreSQL
    }

    public enum JoinType
    {
        Join,
        InnerJoin,
        LeftJoin,
        RightJoin,
    }

    public enum SqlFunction
    {
        None = 1,
        Count = 2,
        Max = 3,
        Min = 4,
        Avg = 5,
        Sum = 6,
    }
}
