using System.Linq;
using IBaseFramework.AutoMapper;
using IBaseFramework.Base;
using IBaseFramework.BaseService;
using IBaseFramework.DapperExtension;
using IBaseFramework.IdHelper;
using IBaseFramework.Utility.Extension;
using FrameworkDemo.Domain.DbContext;
using FrameworkDemo.DomainDto.User;
using FrameworkDemo.Entity;

namespace FrameworkDemo.Domain.Service
{
    /// <summary>
    /// 用户 服务接口
    /// </summary>
    public partial interface IUserService : IBaseService<UserRequest, UserResponse, UserSearch>, IDependency
    {

    }

    /// <summary>
    /// 用户 服务实现
    /// </summary>
    public partial class UserService : BaseService, IUserService
    {
        public UserService(IDbSession dbSession) : base(dbSession)
        {
        }

        #region 添加 用户
        /// <summary>
        /// 添加 用户
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public Result<long> Add(UserRequest requestModel)
        {
            if (requestModel == null)
                return Result.Error<long>(MsgParameterError);

            var newUser = new User()
            {
				UserId=IdHelper.Instance.LongId,
				Name=requestModel.Name,
				Age=requestModel.Age,

            };

            var result = DbSession.UserRepository.Add(newUser);
            return result ?  Result.Success(newUser.UserId) : Result.Error<long>(MsgAddError);
        }
        #endregion

        #region 修改 用户
        /// <summary>
        /// 修改 用户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public Result Edit(long id, UserRequest requestModel)
        {
            if (id < 1 || requestModel == null)
                return Result.Error(MsgParameterError);

            var currentUser = DbSession.UserRepository.Get(id);
            if (currentUser == null)
                return Result.Error("该用户不存在！");

			currentUser.Name=requestModel.Name;
			currentUser.Age=requestModel.Age;


            var result = DbSession.UserRepository.Update(currentUser);
            return result ?  Result.Success(MsgEditSuccess) : Result.Error(MsgEditError);
        }
        #endregion

        #region 删除 用户
        /// <summary>
        /// 删除 用户
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public Result Delete(params long[] ids)
        {
            if (ids == null || ids.Length < 1)
                return Result.Error(MsgParameterError);

            bool result;
            if (ids.Length == 1)
            {
                var id = ids.First();
                result = DbSession.UserRepository.Delete(u => u.UserId == id);
            }
            else
            {
                result = DbSession.UserRepository.Delete(u => ids.Contains(u.UserId));
            }

            return result ?  Result.Success(MsgDeleteSuccess) : Result.Error(MsgDeleteError);
        }
        #endregion

        #region 查询 用户
        /// <summary>
        /// 根据Id查询 用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Result<UserResponse> GetById(long id)
        {
            if (id < 0)
                return Result.Error<UserResponse>(MsgParameterError);

            var result = DbSession.UserRepository.Get(id);
            return  Result.Success(result.MapTo<UserResponse>());
        }

        /// <summary>
        /// 分页查询 用户
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public Result<PagedList<UserResponse>> GetByPage(UserSearch search)
        {
            if (search == null)
                return Result.Error<PagedList<UserResponse>>(MsgParameterError);

            var condition = DbSession.UserRepository.ExpressionTrue();
            condition = condition.And(u => u.IsDeleted == false);

            
			if (search.Age > 0)
				condition = condition.And(u => u.Age == search.Age);

			if (!string.IsNullOrEmpty(search.Name))
				condition = condition.And(u => u.Name.StartsWith(search.Name));




            var result = DbSession.UserRepository.GetPageList(condition, search.PageIndex, search.PageSize, "CreateTime desc");
            return  Result.Success(result.MapTo<PagedList<UserResponse>>());
        }
        #endregion
    }
}
