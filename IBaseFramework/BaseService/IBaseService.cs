using IBaseFramework.Base;

namespace IBaseFramework.BaseService
{
    public interface IBaseService<in TRequest, TResponse, in TSearch>
        where TRequest : IRequestBase, new()
        where TResponse : IResponseBase, new()
        where TSearch : IRequestBase, new()
    {
        Result<long> Add(TRequest requestModel);
        Result Edit(long id, TRequest requestModel);
        Result Delete(params long[] ids);
        Result<TResponse> GetById(long id);
        Result<PagedList<TResponse>> GetByPage(TSearch search);
    }
}