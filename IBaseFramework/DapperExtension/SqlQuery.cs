using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBaseFramework.DapperExtension
{
    /// <summary>
    ///     A object with the generated sql and dynamic params.
    /// </summary>
    public class SqlQuery
    {
        /// <summary>
        ///     Initializes a new instance of the class.
        /// </summary>
        public SqlQuery()
        {
            SqlBuilder = new StringBuilder();
            Param = new Dictionary<string, object>();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the class.
        /// </summary>
        /// <param name="param">The param.</param>
        public SqlQuery(object param)
            : this()
        {
            SetParam(param);
        }

        /// <summary>
        ///     SqlBuilder
        /// </summary>
        public StringBuilder SqlBuilder { get; }

        /// <summary>
        ///     Gets the param
        /// </summary>
        public Dictionary<string, object> Param { get; }

        /// <summary>
        ///     Gets the SQL.
        /// </summary>
        public string GetSql()
        {
            return SqlBuilder.ToString().Trim();
        }

        /// <summary>
        ///     Set alternative param
        /// </summary>
        /// <param name="param">The param.</param>
        public void SetParam(object param)
        {
            if (param is Dictionary<string, object> paraDic)
            {
                foreach (var prop in paraDic)
                {
                    if (Param.ContainsKey(prop.Key))
                    {
                        continue;
                    }
                    Param.Add(prop.Key, prop.Value);
                }
            }
            else
            {
                var props = param.GetType().GetProperties();
                if (!props.Any())
                    return;

                foreach (var prop in props)
                {
                    if (Param.ContainsKey(prop.Name))
                    {
                        continue;
                    }
                    Param.Add(prop.Name, prop.GetValue(param));
                }
            }
        }
    }
}