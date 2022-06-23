namespace IBaseFramework.Base
{
    public interface IUserManagerProvider : IDependency
    {
        object GetUserId();
    }
}
