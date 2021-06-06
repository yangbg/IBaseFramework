using System;
using System.Linq.Expressions;
using IBaseFramework.Base;
using IBaseFramework.DapperExtension;
using IBaseFramework.Ioc;

namespace IBaseFramework.Infrastructure
{
    public partial class Repository<TEntity> : IRepository<TEntity> where TEntity : EntityBase
    {
        private ISqlGenerator<TEntity> _sqlGenerator = null;
        private ISqlGenerator<TEntity> SqlGenerator
        {
            get
            {
                if (_sqlGenerator != null)
                    return _sqlGenerator;

                _sqlGenerator = new SqlGenerator<TEntity>(new SqlGeneratorConfig(DapperContext.DatabaseConfiguration.SqlProvider));

                return _sqlGenerator;
            }
        }

        [ThreadStatic]
        static IUserManagerProvider _userManager;
        private static dynamic UserId
        {
            get
            {
                if (!DapperContext.DatabaseConfiguration.RegisterUserId)
                    return 0;

                if (_userManager != null)
                    return _userManager.GetUserId();

                _userManager = IocManager.Resolve<IUserManagerProvider>();
                if (_userManager == null)
                {
                    throw new Exception("Please implement the interface IUserManagerProvider!");
                }

                return _userManager.GetUserId();
            }
        }

        public Expression<Func<TEntity, bool>> ExpressionTrue()
        {
            return f => true;
        }

        private bool SetValue(long newId, TEntity instance)
        {
            var added = newId > 0;
            if (added)
            {
                var newParsedId = Convert.ChangeType(newId, SqlGenerator.IdentitySqlProperty.PropertyType);
                SqlGenerator.IdentitySqlProperty.SetValue(instance, newParsedId);
            }
            return added;
        }
    }
}