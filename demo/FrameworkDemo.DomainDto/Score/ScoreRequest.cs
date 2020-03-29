using IBaseFramework.BaseService;

namespace FrameworkDemo.DomainDto.Score
{
    /// <summary>
	/// 成绩 请求实体(添加/修改)
	/// </summary>
    public class ScoreRequest : RequestBase
    {
		public long Id { get; set; }

		public long UserId { get; set; }

		public decimal ScoreData { get; set; }


    }
}
