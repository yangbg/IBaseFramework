using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac;
using FrameworkDemo.Domain.DbContext;
using FrameworkDemo.Domain.Service;
using FrameworkDemo.Entity;
using IBaseFramework.CodeFactory;
using IBaseFramework.DapperExtension;
using IBaseFramework.IdHelper;
using IBaseFramework.Infrastructure;
using IBaseFramework.Ioc;
using IBaseFramework.Utility.Extension;
using NUnit.Framework;

namespace FrameworkDemo.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void ServiceTest()
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
            IocRegister.Register("FrameworkDemo");

            var dbSession = IocManager.Resolve<IDbSession>();

            dbSession.Transaction(dt =>
            {
                dbSession.UserRepository.Add(new User()
                {
                    UserId = IdHelper.Instance.LongId,
                    Name = "测试",
                    Age = 18
                });
                dbSession.ScoreRepository.Add(new Score()
                {
                    Id = IdHelper.Instance.LongId,
                    ScoreData = 22,
                    UserId = IdHelper.Instance.LongId
                });
            });

        }

        [Test]
        //可选，创建service，dto
        public void CreateDomainService()
        {
            CodeFactory.MakeDomainService("Score", "成绩", "FrameworkDemo", @"../../../../FrameworkDemo.Domain", true, true);
        }

        [Test]
        //第二步，创建数据操作统一入口
        public void CreateDbSession()
        {
            CodeFactory.MakeDbSession("FrameworkDemo.Entity", "FrameworkDemo", @"../../../../FrameworkDemo.Domain");
        }

        [Test]
        //第一步，创建实体
        public void CreateEntity()
        {
            var connectionStr =
                "server=127.0.0.1;user id=root;password=1234;database=test;allowuservariables=True;SslMode=none;";
            var dataBase = "test";

            CodeFactory.MakeEntity(connectionStr, dataBase, "FrameworkDemo", @"../../../../FrameworkDemo.Entity");
        }
    }
}