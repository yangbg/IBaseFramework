using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;

namespace IBaseFramework.Ioc
{
    public class IocManager
    {
        public static T Resolve<T>()
        {
            return IocRegister.Container.IsRegistered<T>() ? IocRegister.Container.Resolve<T>() : default;
        }

        public static T Resolve<T>(Dictionary<Type, object> parametersDic)
        {
            var parameterList = new List<Parameter>();

            if (parametersDic == null || parametersDic.Count <= 0)
                return IocRegister.Container.Resolve<T>(parameterList);

            parameterList.AddRange(parametersDic.Select(paras => new TypedParameter(paras.Key, paras.Value)));

            return IocRegister.Container.Resolve<T>(parameterList);
        }
    }
}
