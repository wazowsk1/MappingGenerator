using System;
using System.Diagnostics;

namespace MappingGenerator.OnBuildGenerator
{
    [AttributeUsage(AttributeTargets.Interface)]
    [Conditional("CodeGeneration")]
    public class MappingInterface : Attribute
    {
        public MappingInterface(params System.Type[] typeMappers)
        {
            TypeMappers = typeMappers;
        }

        public System.Type[] TypeMappers { get; }
    }
}