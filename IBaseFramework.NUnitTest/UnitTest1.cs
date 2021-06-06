using System;
using System.Text;
using Autofac;
using IBaseFramework.DapperExtension;
using IBaseFramework.Infrastructure;
using IBaseFramework.Ioc;
using IBaseFramework.NUnitTest.DbContext;
using IBaseFramework.NUnitTest.Entity;
using IBaseFramework.Utility.Helper;
using NUnit.Framework;

namespace IBaseFramework.NUnitTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            DapperContext.DatabaseConfigurationEventHandle += config =>
            {
                config.SqlProvider = SqlProvider.MySQL;
                config.ConnectionString = "server=127.0.0.1;user id=root;password=1234;database=test;allowuservariables=True;SslMode=none;";
                config.RegisterUserId = false;
            };

            IocRegister.RegisterEventHandler += builder =>
            {
                builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            };
            IocRegister.Register("IBaseFramework.NUnitTest");
        }

        [Test]
        public void Test1()
        {
            var dbSession = IocManager.Resolve<IDbSession>();

            dbSession.UserRepository.Add(new User()
            {
                UserId = IdHelper.IdHelper.LongId,
                Name = "name1",
                Age = 10
            });

            dbSession.UserRepository.Add(new User()
            {
                UserId = IdHelper.IdHelper.LongId,
                Name = "name2",
                Age = 12
            });

            dbSession.UserRepository.Update(u => u.UserId == 1, u => new User { Name = "sss", Age = 1 });

            // var users = dbSession.UserRepository.GetList(u => true, u => new { u.Name, u.UserId });
            var users = dbSession.Page<User>("select * from user", null, 1, 2);
            Console.WriteLine(users.TotalCount);
            foreach (var item in users.Items)
            {
                System.Console.WriteLine(item.Name + "_" + item.UserId + '_' + item.Age);
            }

            var user = dbSession.UserRepository.Get(1);
            System.Console.WriteLine(user.Name + "_" + user.UserId + '_' + user.Age);


            Console.WriteLine(SecurityHelper.Md5Encrypt("123456", Encoding.UTF8).GetAwaiter().GetResult());
        }
    }
}