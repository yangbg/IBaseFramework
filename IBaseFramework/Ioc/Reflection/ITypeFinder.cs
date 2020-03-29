using System;

namespace IBaseFramework.Ioc.Reflection
{
    /// <summary> 类型查找器 </summary>
    public interface ITypeFinder
    {
        Type[] Find(Func<Type, bool> expression);

        Type[] FindAll();
    }
}
