using Mapster;

namespace IBaseFramework.Mapper
{
    public static class MapsterMapperExtension
    {
        /// <summary>
        /// 对象对对象
        /// </summary>
        /// <returns></returns>
        public static TTarget MapTo<TTarget>(this object source)
        {
            return source == null ? default : source.Adapt<TTarget>();
        }
    }
}
