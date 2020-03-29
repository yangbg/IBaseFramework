using IBaseFramework.Base;
using IBaseFramework.Infrastructure;
using FrameworkDemo.Entity;
namespace FrameworkDemo.Domain.DbContext
{
    public class DbSession : UnitOfWork, IDbSession
    {
		/// <summary>
		///成绩
		/// </summary>
		public IRepository<Score> ScoreRepository => GetRepository<Score>();
		/// <summary>
		///用户
		/// </summary>
		public IRepository<User> UserRepository => GetRepository<User>();

    }

    public interface IDbSession : IUnitOfWork, IDependency
    {
		/// <summary>
		///成绩
		/// </summary>
		IRepository<Score> ScoreRepository{get;}
		/// <summary>
		///用户
		/// </summary>
		IRepository<User> UserRepository{get;}

    }
}

