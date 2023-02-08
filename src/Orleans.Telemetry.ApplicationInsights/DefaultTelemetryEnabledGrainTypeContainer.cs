using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleans.Telemetry.ApplicationInsights
{
    /// <summary>
    /// Specifies which grains are included in the telemetry
    /// </summary>
    public interface ITelemetryEnabledGrainTypeContainer
    {
        /// <summary>
        /// Determine whether a grain is included in the telemetry
        /// </summary>
        /// <param name="grain">Type of the grain to check</param>
        /// <returns>true if the instance registered the provided type as a grain included in the telemetry</returns>
        bool IncludeInTelemetry(Type grain);
    }

    /// <summary>
    /// Specifies which grains are included in the telemetry by discovering them in a set of assemblies
    /// </summary>
    public class DefaultTelemetryEnabledGrainTypeContainer : ITelemetryEnabledGrainTypeContainer
    {
        private readonly IEnumerable<Type> _grainTypes;

        /// <summary>
        /// Creates a new DefaultTelemetryEnabledGrainTypeContainer that includes all grains found in any of the assemblies
        /// that are available from the set obtained by calling Domain.CurrentDomain.GetAssemblies()
        /// </summary>
        public DefaultTelemetryEnabledGrainTypeContainer() : this(assemblies: null)
        {
            
        }

        /// <summary>
        /// Creates a new DefaultTelemetryEnabledGrainTypeContainer that includes all grains found in any of the assemblies
        /// that are available from the specified assembly
        /// </summary>
        /// <param name="assembly">Grains found in this assembly will be included in the telemetry</param>
        public DefaultTelemetryEnabledGrainTypeContainer(Assembly assembly) : this(new[] { assembly })
        {
        }

        /// <summary>
        /// Creates a new DefaultTelemetryEnabledGrainTypeContainer that includes all grains found in any of the assemblies
        /// that are available from the specified assemblies
        /// </summary>
        /// <param name="assemblies">Grains found in these assemblies will be included in the telemetry</param>
        public DefaultTelemetryEnabledGrainTypeContainer(Assembly[] assemblies)
        {
            _grainTypes = (assemblies ?? AppDomain.CurrentDomain.GetAssemblies())
                .SelectMany(a => a.GetTypes())
                .Where(type => typeof(IGrain).IsAssignableFrom(type))
                .ToList();
        }

        /// <summary>
        /// Determine whether a grain is included in the telemetry
        /// </summary>
        /// <param name="grain">Type of the grain to check</param>
        /// <returns>true if this instance registered the provided type as a grain included in the telemetry</returns>
        public bool IncludeInTelemetry(Type grain)
        {
            return _grainTypes.Contains(grain);
        }
    }
}