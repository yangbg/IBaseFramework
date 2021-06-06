using System;
using IBaseFramework.Ioc.Reflection;

namespace IBaseFramework.Ioc
{
    public class AssemblyFinder : DAssemblyFinder
    {
        public AssemblyFinder(string startsWithName)
        {
            DefaultPredicate = t => t.FullName.StartsWith(startsWithName, StringComparison.CurrentCultureIgnoreCase);
        }

        public static AssemblyFinder Instance(string startsWithName)
        {
            return new AssemblyFinder(startsWithName);
        }

    }
}
