using IBaseFramework.Base;
using IBaseFramework.Id;

namespace IBaseFramework.NUnitTest.DbContext
{
    public class UserSession : IUserManagerProvider
    {
        public object GetUserId()
        {
            return IdHelper.LongId;
        }
    }
}