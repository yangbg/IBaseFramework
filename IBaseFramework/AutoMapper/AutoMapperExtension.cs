using AutoMapper;

namespace IBaseFramework.AutoMapper
{
    public static class AutoMapperExtension
    {
        /// <summary>   
        /// 对象对对象   
        /// </summary>
        /// <returns></returns>  
        public static TTarget MapTo<TTarget>(this object source)
        {
            if (source == null)
                return default(TTarget);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMissingTypeMaps = true;
                cfg.ValidateInlineMaps = false;
            });
            var mapper = config.CreateMapper();

            return mapper.Map<TTarget>(source);
        }

    }

}
