using FrameworkDemo.Domain.DbContext;

namespace FrameworkDemo.Domain
{
    public class BaseService
    {
        public IDbSession DbSession { get; }

        public BaseService(IDbSession dbSession)
        {
            DbSession = dbSession;
        }

        public const string MsgParameterError = "参数错误!";
        public const string MsgAddSuccess = "添加成功!";
        public const string MsgAddError = "添加失败，请稍后重试!";
        public const string MsgEditSuccess = "编辑成功!";
        public const string MsgEditError = "编辑失败，请稍后重试!";
        public const string MsgDeleteSuccess = "删除成功!";
        public const string MsgDeleteError = "删除失败，请稍后重试!";
        public const string MsgSuccess = "操作成功!";
        public const string MsgError = "操作失败，请稍后重试!";

    }
}