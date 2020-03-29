using IBaseFramework.BaseService;

namespace FrameworkDemo.DomainDto.User
{
    /// <summary>
	/// 用户 查询实体
	/// </summary>
    public class UserSearch : Pager
    {
		public long UserId { get; set; }

		public string Name { get; set; }

		public int Age { get; set; }


    }
}
