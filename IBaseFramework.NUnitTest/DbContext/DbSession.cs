using IBaseFramework.Base;
using IBaseFramework.Infrastructure;
using IBaseFramework.NUnitTest.Entity;

namespace IBaseFramework.NUnitTest.DbContext
{
    public class DbSession : UnitOfWork, IDbSession
    {
		/// <summary>
		///用户
		/// </summary>
		public IRepository<User> UserRepository => GetRepository<User>();

    }

    public interface IDbSession : IUnitOfWork, IDependency
    {
		/// <summary>
		///用户
		/// </summary>
		IRepository<User> UserRepository{get;}

    }
}

