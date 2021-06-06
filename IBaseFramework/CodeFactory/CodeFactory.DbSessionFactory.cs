using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IBaseFramework.CodeFactory
{
    /// <summary>
    /// DbSession 生成工厂
    /// </summary>
    public partial class CodeFactory
    {
        #region 生成 DbSession
        /// <summary>
        /// 生成 DbSession
        /// </summary>
        /// <param name="entityLibName">实体程序集名称</param>
        /// <param name="nameSpacePrefix">生成的命名空间前缀</param>
        /// <param name="projectPath">生成到哪个项目目录</param>
        public static void MakeDbSession(string entityLibName, string nameSpacePrefix, string projectPath)
        {
            if (string.IsNullOrEmpty(entityLibName))
                throw new Exception("entityLibName can not be empty!");

            if (string.IsNullOrEmpty(projectPath))
                throw new Exception("projectPath can not be empty!");

            #region BaseService

            var baseServiceStr = $@"using {nameSpacePrefix}.Domain.DbContext;

namespace {nameSpacePrefix}.Domain
{{
    public class BaseService
    {{
        public IDbSession DbSession {{ get; }}

        public BaseService(IDbSession dbSession)
        {{
            DbSession = dbSession;
        }}

        public const string MsgParameterError = ""参数错误!"";
        public const string MsgAddSuccess = ""添加成功!"";
        public const string MsgAddError = ""添加失败，请稍后重试!"";
        public const string MsgEditSuccess = ""编辑成功!"";
        public const string MsgEditError = ""编辑失败，请稍后重试!"";
        public const string MsgDeleteSuccess = ""删除成功!"";
        public const string MsgDeleteError = ""删除失败，请稍后重试!"";
        public const string MsgSuccess = ""操作成功!"";
        public const string MsgError = ""操作失败，请稍后重试!"";

    }}
}}";
            var filePath2 = Path.Combine(projectPath, "BaseService.cs");
            if (File.Exists(filePath2))
            {
                File.Delete(filePath2);
            }

            File.AppendAllText(filePath2, baseServiceStr, Encoding.UTF8);

            #endregion

            #region DbSession
            var lib = AppDomain.CurrentDomain.Load(entityLibName);

            var classList = lib?.GetTypes()?.Where(u => u.Name != "M").ToList();

            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            if (classList.Count > 0)
            {
                foreach (var type in classList)
                {
                    var name = type.Name;
                    var comment = string.Empty;
                    var attr = type.GetCustomAttribute<DescriptionAttribute>();
                    if (attr != null)
                    {
                        comment = attr.Description;
                    }

                    sb1.AppendLine("\t\t/// <summary>");
                    sb1.AppendLine("\t\t///" + comment);
                    sb1.AppendLine("\t\t/// </summary>");
                    sb1.AppendLine($"\t\tpublic IRepository<{name}> {name}Repository => GetRepository<{name}>();");

                    sb2.AppendLine("\t\t/// <summary>");
                    sb2.AppendLine("\t\t///" + comment);
                    sb2.AppendLine("\t\t/// </summary>");
                    sb2.AppendLine($"\t\tIRepository<{name}> {name}Repository{{get;}}");
                }
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($@"using IBaseFramework.Base;
using IBaseFramework.Infrastructure;
using {entityLibName};
namespace {nameSpacePrefix}.Domain.DbContext
{{
    public class DbSession : UnitOfWork, IDbSession
    {{
{sb1}
    }}

    public interface IDbSession : IUnitOfWork, IDependency
    {{
{sb2}
    }}
}}
");
            projectPath = Path.Combine(projectPath, "DbContext");
            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
            }

            var filePath = Path.Combine(projectPath, "DbSession.cs");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.AppendAllText(filePath, stringBuilder.ToString(), Encoding.UTF8);
            #endregion
            
//            #region DbContext
//            var dbContextStr = $@"using IBaseFramework.Base;
//using IBaseFramework.Infrastructure;

//namespace {nameSpacePrefix}.Domain.DbContext
//{{
//    public class DbContext : DapperContext, IDependency
//    {{
//        //TODO:设置数据库连接字符串
//        public DbContext() : base("""", true)
//        {{
//        }}
//    }}
//}}";

//            var filePath1 = Path.Combine(projectPath, "DbContext.cs");
//            if (File.Exists(filePath1))
//            {
//                File.Delete(filePath1);
//            }

//            File.AppendAllText(filePath1, dbContextStr, Encoding.UTF8); 
//            #endregion
        }
        #endregion
    }
}