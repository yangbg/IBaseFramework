using IBaseFramework.Base;
using IBaseFramework.DapperExtension;
using System.ComponentModel;
namespace FrameworkDemo.Entity
{
	[Table("User")]
	[Description("用户")]
	public class User : EntityBase
	{
		public static class M
		{
			public const string TableName = "User";
			public const string UserId = "UserId";
			public const string Name = "Name";
			public const string Age = "Age";
			public const string Version = "Version";
			public const string CreateUserId = "CreateUserId";
			public const string CreateTime = "CreateTime";
			public const string UpdateUserId = "UpdateUserId";
			public const string UpdateTime = "UpdateTime";
			public const string IsDeleted = "IsDeleted";
		}

		[Key]
		public long UserId { get; set; }

		public string Name { get; set; }

		public int Age { get; set; }

	}
}
