using System.Linq;
using IBaseFramework.AutoMapper;
using IBaseFramework.Base;
using IBaseFramework.BaseService;
using IBaseFramework.DapperExtension;
using IBaseFramework.IdHelper;
using IBaseFramework.Utility.Extension;
using FrameworkDemo.Domain.DbContext;
using FrameworkDemo.DomainDto.Score;
using FrameworkDemo.Entity;

namespace FrameworkDemo.Domain.Service
{
    /// <summary>
    /// 成绩 服务接口
    /// </summary>
    public partial interface IScoreService : IBaseService<ScoreRequest, ScoreResponse, ScoreSearch>, IDependency
    {

    }

    /// <summary>
    /// 成绩 服务实现
    /// </summary>
    public partial class ScoreService : BaseService, IScoreService
    {
        public ScoreService(IDbSession dbSession) : base(dbSession)
        {
        }

        #region 添加 成绩
        /// <summary>
        /// 添加 成绩
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public Result<long> Add(ScoreRequest requestModel)
        {
            if (requestModel == null)
                return Result.Error<long>(MsgParameterError);

            var newScore = new Score()
            {
				Id=IdHelper.Instance.LongId,
				UserId=requestModel.UserId,
				ScoreData=requestModel.ScoreData,

            };

            var result = DbSession.ScoreRepository.Add(newScore);
            return result ?  Result.Success(newScore.Id) : Result.Error<long>(MsgAddError);
        }
        #endregion

        #region 修改 成绩
        /// <summary>
        /// 修改 成绩
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public Result Edit(long id, ScoreRequest requestModel)
        {
            if (id < 1 || requestModel == null)
                return Result.Error(MsgParameterError);

            var currentScore = DbSession.ScoreRepository.Get(id);
            if (currentScore == null)
                return Result.Error("该成绩不存在！");

			currentScore.UserId=requestModel.UserId;
			currentScore.ScoreData=requestModel.ScoreData;


            var result = DbSession.ScoreRepository.Update(currentScore);
            return result ?  Result.Success(MsgEditSuccess) : Result.Error(MsgEditError);
        }
        #endregion

        #region 删除 成绩
        /// <summary>
        /// 删除 成绩
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
                result = DbSession.ScoreRepository.Delete(u => u.Id == id);
            }
            else
            {
                result = DbSession.ScoreRepository.Delete(u => ids.Contains(u.Id));
            }

            return result ?  Result.Success(MsgDeleteSuccess) : Result.Error(MsgDeleteError);
        }
        #endregion

        #region 查询 成绩
        /// <summary>
        /// 根据Id查询 成绩
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Result<ScoreResponse> GetById(long id)
        {
            if (id < 0)
                return Result.Error<ScoreResponse>(MsgParameterError);

            var result = DbSession.ScoreRepository.Get(id);
            return  Result.Success(result.MapTo<ScoreResponse>());
        }

        /// <summary>
        /// 分页查询 成绩
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public Result<PagedList<ScoreResponse>> GetByPage(ScoreSearch search)
        {
            if (search == null)
                return Result.Error<PagedList<ScoreResponse>>(MsgParameterError);

            var condition = DbSession.ScoreRepository.ExpressionTrue();
            condition = condition.And(u => u.IsDeleted == false);

            			if (search.UserId > 0)
				condition = condition.And(u => u.UserId == search.UserId);

			if (search.ScoreData > 0)
				condition = condition.And(u => u.ScoreData == search.ScoreData);





            var result = DbSession.ScoreRepository.GetPageList(condition, search.PageIndex, search.PageSize, "CreateTime desc");
            return  Result.Success(result.MapTo<PagedList<ScoreResponse>>());
        }
        #endregion
    }
}
