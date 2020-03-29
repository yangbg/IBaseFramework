using IBaseFramework.DapperExtension;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IBaseFramework.CodeFactory
{
    /// <summary>
    /// DomainService 生成工厂
    /// </summary>
    public partial class CodeFactory
    {
        #region 生成DomainService
        /// <summary>
        /// 生成DomainService
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <param name="remark">实体备注</param>
        /// <param name="nameSpacePrefix">命名空间前缀(eg:XXX.Domain)</param>
        /// <param name="projectPath">service生成到哪个项目目录</param>
        /// <param name="createService">是否创建service</param>
        /// <param name="createDomainObject">是否创建domainObject</param>
        public static void MakeDomainService(string entityName, string remark, string nameSpacePrefix, string projectPath, bool createService = true, bool createDomainObject = true)
        {
            if (string.IsNullOrEmpty(entityName))
                throw new Exception("entityName can not be empty!");

            if (string.IsNullOrEmpty(projectPath))
                throw new Exception("projectPath can not be empty!");

            if (string.IsNullOrEmpty(nameSpacePrefix))
                throw new Exception("nameSpacePrefix can not be empty!");

            var entityLibName = nameSpacePrefix + ".Entity";
            var domainLibName = nameSpacePrefix + ".Domain";

            var lib = AppDomain.CurrentDomain.Load(entityLibName);
            var type = lib?.GetTypes()?.FirstOrDefault(u => u.Name == entityName);
            if (type == null)
                throw new Exception("entity not found!");

            var props = type.GetProperties();

            var key = props.FirstOrDefault(u => u.CustomAttributes.Any(a => a.AttributeType == typeof(KeyAttribute)));

            var sb = new StringBuilder();

            var addSb = new StringBuilder();
            foreach (var prop in props)
            {
                if (CommonColumns.Contains(prop.Name.ToLower())) { continue; }

                if (key != null && prop.Name == key.Name)
                {
                    addSb.AppendLine($"\t\t\t\t{prop.Name}=IdHelper.Instance.LongId,");
                }
                else
                {
                    addSb.AppendLine($"\t\t\t\t{prop.Name}=requestModel.{prop.Name},");
                }
            }

            var editSb = new StringBuilder();
            foreach (var prop in props)
            {
                if (CommonColumns.Contains(prop.Name.ToLower()) || (key != null && prop.Name == key.Name)) { continue; }

                editSb.AppendLine($"\t\t\tcurrent{entityName}.{prop.Name}=requestModel.{prop.Name};");
            }

            var searchSb = new StringBuilder();
            foreach (var prop in props)
            {
                if (CommonColumns.Contains(prop.Name.ToLower()) || (key != null && prop.Name == key.Name)) { continue; }

                if (prop.PropertyType.IsValueType)//值类型
                {
                    searchSb.AppendLine($"\t\t\tif (search.{prop.Name} > 0)");
                    searchSb.AppendLine($"\t\t\t\tcondition = condition.And(u => u.{prop.Name} == search.{prop.Name});");
                }

                searchSb.AppendLine();
            }

            foreach (var prop in props)
            {
                if (CommonColumns.Contains(prop.Name.ToLower()) || (key != null && prop.Name == key.Name)) { continue; }

                if (!prop.PropertyType.IsValueType)//非值类型
                {
                    searchSb.AppendLine($"\t\t\tif (!string.IsNullOrEmpty(search.{prop.Name}))");
                    searchSb.AppendLine($"\t\t\t\tcondition = condition.And(u => u.{prop.Name}.StartsWith(search.{prop.Name}));");
                }

                searchSb.AppendLine();
            }

            #region DomainContent
            sb.Append($@"using System.Linq;
using IBaseFramework.AutoMapper;
using IBaseFramework.Base;
using IBaseFramework.BaseService;
using IBaseFramework.DapperExtension;
using IBaseFramework.IdHelper;
using IBaseFramework.Utility.Extension;
using {nameSpacePrefix}.Domain.DbContext;
using {nameSpacePrefix}.DomainDto.{entityName};
using {nameSpacePrefix}.Entity;

namespace {nameSpacePrefix}.Domain.Service
{{
    /// <summary>
    /// {remark} 服务接口
    /// </summary>
    public partial interface I{entityName}Service : IBaseService<{entityName}Request, {entityName}Response, {entityName}Search>, IDependency
    {{

    }}

    /// <summary>
    /// {remark} 服务实现
    /// </summary>
    public partial class {entityName}Service : BaseService, I{entityName}Service
    {{
        public {entityName}Service(IDbSession dbSession) : base(dbSession)
        {{
        }}

        #region 添加 {remark}
        /// <summary>
        /// 添加 {remark}
        /// </summary>
        /// <param name=""requestModel""></param>
        /// <returns></returns>
        public Result<long> Add({entityName}Request requestModel)
        {{
            if (requestModel == null)
                return Result.Error<long>(MsgParameterError);

            var new{entityName} = new {entityName}()
            {{
{addSb}
            }};

            var result = DbSession.{entityName}Repository.Add(new{entityName});
            return result ?  Result.Success(new{entityName}.{(key == null ? "Id" : key.Name)}) : Result.Error<long>(MsgAddError);
        }}
        #endregion

        #region 修改 {remark}
        /// <summary>
        /// 修改 {remark}
        /// </summary>
        /// <param name=""id""></param>
        /// <param name=""requestModel""></param>
        /// <returns></returns>
        public Result Edit(long id, {entityName}Request requestModel)
        {{
            if (id < 1 || requestModel == null)
                return Result.Error(MsgParameterError);

            var current{entityName} = DbSession.{entityName}Repository.Get(id);
            if (current{entityName} == null)
                return Result.Error(""该{remark}不存在！"");

{editSb}

            var result = DbSession.{entityName}Repository.Update(current{entityName});
            return result ?  Result.Success(MsgEditSuccess) : Result.Error(MsgEditError);
        }}
        #endregion

        #region 删除 {remark}
        /// <summary>
        /// 删除 {remark}
        /// </summary>
        /// <param name=""ids""></param>
        /// <returns></returns>
        public Result Delete(params long[] ids)
        {{
            if (ids == null || ids.Length < 1)
                return Result.Error(MsgParameterError);

            bool result;
            if (ids.Length == 1)
            {{
                var id = ids.First();
                result = DbSession.{entityName}Repository.Delete(u => u.{(key == null ? "Id" : key.Name)} == id);
            }}
            else
            {{
                result = DbSession.{entityName}Repository.Delete(u => ids.Contains(u.{(key == null ? "Id" : key.Name)}));
            }}

            return result ?  Result.Success(MsgDeleteSuccess) : Result.Error(MsgDeleteError);
        }}
        #endregion

        #region 查询 {remark}
        /// <summary>
        /// 根据Id查询 {remark}
        /// </summary>
        /// <param name=""id""></param>
        /// <returns></returns>
        public Result<{entityName}Response> GetById(long id)
        {{
            if (id < 0)
                return Result.Error<{entityName}Response>(MsgParameterError);

            var result = DbSession.{entityName}Repository.Get(id);
            return  Result.Success(result.MapTo<{entityName}Response>());
        }}

        /// <summary>
        /// 分页查询 {remark}
        /// </summary>
        /// <param name=""search""></param>
        /// <returns></returns>
        public Result<PagedList<{entityName}Response>> GetByPage({entityName}Search search)
        {{
            if (search == null)
                return Result.Error<PagedList<{entityName}Response>>(MsgParameterError);

            var condition = DbSession.{entityName}Repository.ExpressionTrue();
            condition = condition.And(u => u.IsDeleted == false);

            {searchSb}

            var result = DbSession.{entityName}Repository.GetPageList(condition, search.PageIndex, search.PageSize, ""CreateTime desc"");
            return  Result.Success(result.MapTo<PagedList<{entityName}Response>>());
        }}
        #endregion
    }}
}}
");
            #endregion

            var result = sb.ToString();

            projectPath = Path.Combine(projectPath, "Service");
            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
            }

            var filePath = Path.Combine(projectPath, $"{entityName}Service.cs");
            if (!createService || File.Exists(filePath))
            {
                filePath = Path.Combine(projectPath, "DomainTemplate.cs");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                result = "/*" + result + "*/";
            }

            File.AppendAllText(filePath, result, Encoding.UTF8);

            if (createDomainObject)
            {
                MakeClass(entityName, nameSpacePrefix, domainLibName, projectPath, props, remark);
            }
        }

        #endregion

        #region Private
        private static void MakeClass(string entityName, string nameSpacePrefix, string domainLibName, string path, PropertyInfo[] props, string remark)
        {
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            var sb3 = new StringBuilder();

            var propInfos = new StringBuilder();
            foreach (var prop in props)
            {
                if (CommonColumns.Contains(prop.Name.ToLower())) { continue; }

                var comment = prop.GetCustomAttribute<DescriptionAttribute>();
                if (comment != null)
                {
                    propInfos.AppendLine("\t\t/// <summary>");
                    propInfos.AppendLine($"\t\t/// {comment.Description}");
                    propInfos.AppendLine("\t\t/// </summary>");
                }

                propInfos.AppendLine($"\t\tpublic {GetPropType(prop.PropertyType)} {prop.Name} {{ get; set; }}");
                propInfos.AppendLine("");
            }


            //Response
            sb2.AppendLine($@"using IBaseFramework.BaseService;

namespace {nameSpacePrefix}.DomainDto.{entityName}
{{
    /// <summary>
	/// {remark} 返回实体
	/// </summary>
    public class {entityName}Response : ResponseBase
    {{
{propInfos}
    }}
}}");

            //Request
            sb1.AppendLine($@"using IBaseFramework.BaseService;

namespace {nameSpacePrefix}.DomainDto.{entityName}
{{
    /// <summary>
	/// {remark} 请求实体(添加/修改)
	/// </summary>
    public class {entityName}Request : RequestBase
    {{
{propInfos}
    }}
}}");


            //Search
            sb3.AppendLine($@"using IBaseFramework.BaseService;

namespace {nameSpacePrefix}.DomainDto.{entityName}
{{
    /// <summary>
	/// {remark} 查询实体
	/// </summary>
    public class {entityName}Search : Pager
    {{
{propInfos}
    }}
}}");

            var tempPath = path.Substring(0, path.IndexOf(domainLibName, StringComparison.Ordinal));

            path = Path.Combine(tempPath, $"{nameSpacePrefix}.DomainDto", entityName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var filePath = Path.Combine(path, $"{entityName}Request.cs");
            if (!File.Exists(filePath))
            {
                File.AppendAllText(filePath, sb1.ToString(), Encoding.UTF8);
            }

            var filePath1 = Path.Combine(path, $"{entityName}Response.cs");
            if (!File.Exists(filePath1))
            {
                File.AppendAllText(filePath1, sb2.ToString(), Encoding.UTF8);
            }

            var filePath2 = Path.Combine(path, $"{entityName}Search.cs");
            if (!File.Exists(filePath2))
            {
                File.AppendAllText(filePath2, sb3.ToString(), Encoding.UTF8);
            }
        }

        private static string GetPropType(Type propertyType)
        {
            if (propertyType == null)
                return string.Empty;

            if (propertyType == typeof(string))
            {
                return "string";
            }
            else if (propertyType == typeof(Int64?) || propertyType == typeof(Int64))
            {
                return propertyType == typeof(Int64) ? "long" : "long?";
            }
            else if (propertyType == typeof(byte?) || propertyType == typeof(byte))
            {
                return propertyType == typeof(byte) ? "byte" : "byte?";
            }
            else if (propertyType == typeof(short?) || propertyType == typeof(short))
            {
                return propertyType == typeof(short) ? "short" : "short?";
            }
            else if (propertyType == typeof(double?) || propertyType == typeof(double))
            {
                return propertyType == typeof(double) ? "double" : "double?";
            }
            else if (propertyType == typeof(decimal?) || propertyType == typeof(decimal))
            {
                return propertyType == typeof(decimal) ? "decimal" : "decimal?";
            }
            else if (propertyType == typeof(float?) || propertyType == typeof(float))
            {
                return propertyType == typeof(float) ? "float" : "float?";
            }
            else if (propertyType == typeof(bool?) || propertyType == typeof(bool))
            {
                return propertyType == typeof(bool) ? "bool" : "bool?";
            }
            else if (propertyType == typeof(int?) || propertyType == typeof(int))
            {
                return propertyType == typeof(int) ? "int" : "int?";
            }
            else if (propertyType == typeof(long?) || propertyType == typeof(long))
            {
                return propertyType == typeof(long) ? "long" : "long?";
            }
            else if (propertyType == typeof(DateTime?) || propertyType == typeof(DateTime))
            {
                return propertyType == typeof(DateTime) ? "System.DateTime" : "System.DateTime?";
            }

            return propertyType.Name;
        }
        #endregion
    }
}