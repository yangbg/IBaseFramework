using IBaseFramework.Base;
using IBaseFramework.DapperExtension;

namespace IBaseFramework.BaseService
{
    public interface IBaseService<in TRequest, TResponse, in TSearch>
        where TRequest : RequestBase, new()
        where TResponse : ResponseBase, new()
        where TSearch : RequestBase, new()
    {
        Result<long> Add(TRequest requestModel);
        Result Edit(long id, TRequest requestModel);
        Result Delete(params long[] ids);
        Result<TResponse> GetById(long id);
        Result<PagedList<TResponse>> GetByPage(TSearch search);
    }
}