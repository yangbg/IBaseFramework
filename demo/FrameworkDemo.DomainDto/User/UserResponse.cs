using IBaseFramework.BaseService;

namespace FrameworkDemo.DomainDto.User
{
    /// <summary>
	/// 用户 返回实体
	/// </summary>
    public class UserResponse : ResponseBase
    {
		public long UserId { get; set; }

		public string Name { get; set; }

		public int Age { get; set; }


    }
}
