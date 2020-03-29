using IBaseFramework.DapperExtension;

namespace IBaseFramework.Infrastructure
{
    public class DapperContextConfig
    {
        /// <summary>
        ///     ConnectionString
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Type Sql provider
        /// </summary>
        public SqlProvider SqlProvider { get; set; } = SqlProvider.MySQL;

        /// <summary>
        ///     Register UserId(implement the interface IUserManagerProvider)
        /// </summary>
        public bool RegisterUserId { get; set; } = false;

    }
}