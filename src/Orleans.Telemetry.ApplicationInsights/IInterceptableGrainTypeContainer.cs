using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleans.Telemetry.ApplicationInsights
{
    public interface IInterceptableGrainTypeContainer
    {
        bool ContainsGrain(Type grain);
    }

    public class DefaultInterceptableGrainTypeContainer : IInterceptableGrainTypeContainer
    {
        private readonly IEnumerable<Type> _grainTypes;

        public DefaultInterceptableGrainTypeContainer() : this(assemblies: null)
        {
            
        }

        public DefaultInterceptableGrainTypeContainer(Assembly assembly) : this(new[] { assembly })
        {
        }

        public DefaultInterceptableGrainTypeContainer(Assembly[] assemblies)
        {
            _grainTypes = (assemblies ?? AppDomain.CurrentDomain.GetAssemblies())
                .SelectMany(a => a.GetTypes())
                .Where(type => typeof(IGrain).IsAssignableFrom(type))
                .ToList();
        }

        public bool ContainsGrain(Type grain)
        {
            return _grainTypes.Contains(grain);
        }
    }
}