using IBaseFramework.BaseService;

namespace FrameworkDemo.DomainDto.Score
{
    /// <summary>
	/// 成绩 返回实体
	/// </summary>
    public class ScoreResponse : ResponseBase
    {
		public long Id { get; set; }

		public long UserId { get; set; }

		public decimal ScoreData { get; set; }


    }
}
