using IBaseFramework.BaseService;

namespace FrameworkDemo.DomainDto.User
{
    /// <summary>
	/// 用户 请求实体(添加/修改)
	/// </summary>
    public class UserRequest : RequestBase
    {
		public long UserId { get; set; }

		public string Name { get; set; }

		public int Age { get; set; }


    }
}
