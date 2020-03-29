using IBaseFramework.Base;
using IBaseFramework.DapperExtension;
using System.ComponentModel;
namespace FrameworkDemo.Entity
{
	[Table("Score")]
	[Description("成绩")]
	public class Score : EntityBase
	{
		public static class M
		{
			public const string TableName = "Score";
			public const string Id = "Id";
			public const string UserId = "UserId";
			public const string ScoreData = "ScoreData";
			public const string Version = "Version";
			public const string CreateUserId = "CreateUserId";
			public const string CreateTime = "CreateTime";
			public const string UpdateUserId = "UpdateUserId";
			public const string UpdateTime = "UpdateTime";
			public const string IsDeleted = "IsDeleted";
		}

		[Key]
		public long Id { get; set; }

		public long UserId { get; set; }

		public decimal ScoreData { get; set; }

	}
}
