using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

namespace Orleans.Telemetry.ApplicationInsights.Tests
{
    public static class TelemetryCollectionExtensions
    {
        public static DependencyTelemetry GetIncomingGrainMessageTelemetry<T>(this IEnumerable<ITelemetry> telemetry, Guid invocationId, Expression<Func<T, Task>> expression)
        {
            var methodCallExpression = (MethodCallExpression)expression.Body;

            return telemetry.OfType<DependencyTelemetry>().FirstOrDefault(dt =>
                dt.Properties["invocationId"] == invocationId.ToString() &&
                dt.Type == "Orleans Actor MessageIn" &&
                dt.Name == $"{methodCallExpression.Method.DeclaringType?.FullName}.{methodCallExpression.Method.Name}" &&
                dt.Properties["grainType"] == methodCallExpression.Method.DeclaringType?.FullName);
        }

        public static DependencyTelemetry GetOutgoingGrainMessageTelemetry<T>(this IEnumerable<ITelemetry> telemetry, Guid invocationId, Expression<Func<T, Task>> expression)
        {
            var methodCallExpression = (MethodCallExpression)expression.Body;

            return telemetry.OfType<DependencyTelemetry>().FirstOrDefault(dt =>
                dt.Properties["invocationId"] == invocationId.ToString() &&
                dt.Type == "Orleans Actor MessageOut" &&
                dt.Name == $"{methodCallExpression.Method.DeclaringType?.FullName}.{methodCallExpression.Method.Name}" &&
                dt.Properties["grainType"] == methodCallExpression.Method.DeclaringType?.FullName);
        }
    }
}