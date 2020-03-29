using IBaseFramework.BaseService;

namespace FrameworkDemo.DomainDto.Score
{
    /// <summary>
	/// 成绩 查询实体
	/// </summary>
    public class ScoreSearch : Pager
    {
		public long Id { get; set; }

		public long UserId { get; set; }

		public decimal ScoreData { get; set; }


    }
}
