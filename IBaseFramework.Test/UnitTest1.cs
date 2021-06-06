using Autofac;
using IBaseFramework.DapperExtension;
using IBaseFramework.Infrastructure;
using IBaseFramework.Ioc;
using IBaseFramework.Test.DbContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IBaseFramework.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            DapperContext.DatabaseConfigurationEventHandle += config =>
            {
                config.SqlProvider = SqlProvider.MySQL;
                config.ConnectionString = "server=127.0.0.1;user id=root;password=1234;database=test;allowuservariables=True;SslMode=none;";
                config.RegisterUserId = false;
            };

            new DbSession().UserRepository.Update(u => new { Name = "aa", PhoneId = 1 }, u => u.Id == 1);

        }
    }
}
