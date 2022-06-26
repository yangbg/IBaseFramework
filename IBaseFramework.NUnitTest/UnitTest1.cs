using Autofac;
using IBaseFramework.Infrastructure;
using IBaseFramework.Ioc;
using IBaseFramework.NUnitTest.DbContext;
using IBaseFramework.NUnitTest.Entity;
using NUnit.Framework;
using IBaseFramework.Id;
using System.Linq;
using IBaseFramework.Extension;
using System;
using System.Collections.Generic;

namespace IBaseFramework.NUnitTest
{
    public class Tests
    {
        [SetUp] 
        public void Setup()
        {
            DapperContext.DbConfigEventHandle += config =>
            {
                config.SqlProvider = SqlProvider.MySQL;
                config.ConnectionString = "server=127.0.0.1;user id=root;password=1234;database=test;allowuservariables=True;SslMode=none;";
                config.IsRegisterUserId = true;
                config.OnSqlDiagnostics += paras =>
                {
                    if (paras.Sql != null)
                    {
                        Console.WriteLine(paras.Sql);
                        Console.WriteLine(paras.Parameters.ToJson());
                    }
                };
            };

            IocRegister.RegisterEventHandler += builder =>
            {
                builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            };
            IocRegister.Register("IBaseFramework");
        }


        [Test]
        public void Test_Add()
        {
            var dbSession = IocManager.Resolve<IDbSession>();

            for (int i = 0; i < 1; i++)
            {
                dbSession.UserRepository.Add(new User()
                {
                    UserId = IdHelper.LongId,
                    Name = "name1",
                    Age = 10,
                    UserType = UserType.Member
                });

                dbSession.ScoreRepository.Add(new Score()
                {
                    Id = IdHelper.LongId.ToString(),
                    UserId = IdHelper.LongId,
                    ScoreData = 9m
                });

                var userList = Enumerable.Range(1, 1000).Select(u => new User()
                {
                    UserId = IdHelper.LongId,
                    Name = $"name{u}",
                    Age = u / 10,
                    UserType = u % 2 == 0 ? UserType.VIP : UserType.Member
                }).ToList();

                dbSession.UserRepository.Add(userList);


                var scoreList = Enumerable.Range(1, 1000).Select(u => new Score()
                {
                    Id = IdHelper.LongId.ToString(),
                    UserId = IdHelper.LongId,
                    ScoreData = u
                });

                dbSession.ScoreRepository.Add(scoreList);

            }
        }

        [Test]
        public void Test_Update()
        {
            var dbsession = IocManager.Resolve<IDbSession>();

            var users = dbsession.UserRepository.GetList(u => u.Age < 70).ToList();

            var firstUser = users.First();
            firstUser.Name = "我修改了111";

            dbsession.UserRepository.Update(firstUser);

            foreach (var item in users)
            {
                item.Name = "我修改了" + item.Age;
            }

            dbsession.UserRepository.Update(users);

            var userFirst = users.First();
            userFirst.Name = "按条件修改了";

            dbsession.UserRepository.Update(userFirst, u => u.Age == userFirst.Age);

            dbsession.UserRepository.Update(u => u.Age == users.First().Age, u => new User() { Age = users.First().Age }, 12);
        }

        [Test]
        public void Test_Delete()
        {
            var dbsession = IocManager.Resolve<IDbSession>();

            dbsession.UserRepository.Delete(u => u.Age == 6, false, updateUserId: 1);

            var users = dbsession.UserRepository.GetList(u => u.Age < 8).ToList();

            dbsession.UserRepository.Delete(users.First(), false);


            dbsession.UserRepository.Delete(u => true, false);
            dbsession.ScoreRepository.Delete(u => true, false);
        }

        [Test]
        public void Test_Get()
        {
            var dbsession = IocManager.Resolve<IDbSession>();

            //var exist = dbsession.UserRepository.Exist(u => u.Age == 8);
            //System.Console.WriteLine(exist);

            //var user = dbsession.UserRepository.Get(1482629751133835334);
            //System.Console.WriteLine(user.ToJson());

            //var user1 = dbsession.UserRepository.Get(u => u.Name == "22" && u.Age == 7);
            //System.Console.WriteLine(user1.ToJson());

            //var userList = dbsession.UserRepository.GetList(u => u.Name.Contains("我") && (u.Age == 7 || u.Age < 10)).ToList();
            //System.Console.WriteLine(userList.Count);

            //var maxValue = dbsession.UserRepository.Max(u =>u.Age);
            //System.Console.WriteLine(maxValue);

            //var minValue = dbsession.UserRepository.Min(u => u.Age);
            //System.Console.WriteLine(minValue);

            //var sumValue = dbsession.UserRepository.Sum(u => u.Age);
            //System.Console.WriteLine(sumValue);

            //var countValue = dbsession.UserRepository.Where(u => u.Age > 50).Count(u => new { u.Age });
            //System.Console.WriteLine(countValue);

            //var avgValue = dbsession.UserRepository.Avg(u => u.Age);
            //System.Console.WriteLine(avgValue);

            //var user = dbsession.UserRepository.Where(u => u.Age < 20).OrderByDescending(u => u.UserType).OrderBy(u=>u.UserId).OrderByDescending(u => u.Age).FirstOrDefault();
            //System.Console.WriteLine(user.ToJson());

            //var user1 = dbsession.UserRepository.OrderByDescending(u => u.UserType).OrderBy(u => u.UserId).OrderByDescending(u => u.Age).FirstOrDefault(u => u.Age < 20);
            //System.Console.WriteLine(user1.ToJson());

            //var userList = dbsession.UserRepository.Where(u => u.Age < 20).ToList();
            //System.Console.WriteLine(userList.Count());

            //var userList1 = dbsession.UserRepository.Where(u => u.Age < 20).ToPageList(1, 10);
            //System.Console.WriteLine(userList1.Items.Count() + "_" + userList1.TotalCount);

            var user = new User() { Age = 55, Name = "A" };

            //var userList2 = dbsession.UserRepository.Where(u => u.Age < 20).WhereIf(user.Age > 50, u => u.UserType == UserType.VIP, u => u.UserType == UserType.Member).OrderByDescending(u => u.UserType).OrderBy(u => u.UserId).OrderByDescending(u => u.Age).ToPageList(1, 10);
            //System.Console.WriteLine(userList2.Items.Count() + "_" + userList2.TotalCount);

            //var paras = new Parameters();
            //paras.Add("name", "name%");

            //var userList2 = dbsession.UserRepository.Where(u => u.Age > 20).WhereIf(user.Age > 50, u => u.UserType == UserType.VIP, u => u.UserType == UserType.Member).Where("Name like @name", paras).ToPageList(1, 10);
            //System.Console.WriteLine(userList2.Items.Count() + "_" + userList2.TotalCount);

            //var userList3 = dbsession.UserRepository.Where(u => u.Age < 20).OrderByDescending(u => u.Age).Select(u => new { u.Name, u.Age, UserId = u.CreateUserId }).ToPageList(1, 5);
            //System.Console.WriteLine(userList3.ToJson());

            //var userList4 = dbsession.UserRepository.Join<User, Score>((u, s) => u.UserId != s.UserId).Where(u => u.Item1.Age < 20).OrderByDescending(u => u.Item1.Age).Select(u => u.Item1).ToPageList(1, 2);
            //System.Console.WriteLine(userList4.ToJson());

            var userList5 = dbsession.UserRepository.Select(u => "MAX(Age)").FirstOrDefault();
            System.Console.WriteLine(userList5);

            var userNames = new List<string>() { "a", "b", "c" };
            var userIds = new List<long> { 1, 2, 3, 4 };

            var userList3 = dbsession.UserRepository.Where(u => u.IsDeleted && (u.Age != 20 || u.Age > user.Age + 5) && !userNames.Contains(u.Name) && userIds.Contains(u.UserId) && (u.Name.StartsWith(user.Name) || u.Name.EndsWith(user.Name) || u.Name.Contains(user.Name + "B"))).OrderByDescending(u =>u.Age).Select(u => new { u.Name, u.Age, UserId = u.CreateUserId }).ToList();
            System.Console.WriteLine(userList3.ToJson());
        }

        [Test]
        public void Test_Transaction()
        {
            var dbsession = IocManager.Resolve<IDbSession>();

            dbsession.Transaction(() =>
            {
                dbsession.UserRepository.Add(new User()
                {
                    UserId = IdHelper.LongId,
                    Name = "name1",
                    Age = 10,
                    UserType = UserType.Member
                });

                dbsession.ScoreRepository.Add(new Score()
                {
                    Id = IdHelper.Guid32,
                    ScoreData = 33,
                    UserId = 12
                });

                dbsession.UserRepository.Delete(u => true);

                //dbsession.Execute("delete from score ");

                dbsession.UserRepository.Update(u => true, u => new User { Name = "ddd222555" });

                //throw new Exception("出事了");
            });

        }
    }
}