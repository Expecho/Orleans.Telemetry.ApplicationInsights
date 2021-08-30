using System.Threading.Tasks;

namespace Orleans.Telemetry.ApplicationInsights.Tests.Grains
{
    public interface ICalledGrain : IGrainWithIntegerCompoundKey
    {
        Task ReceiveMessage(string message);
    }

    public class CalledGrain : Grain, ICalledGrain
    {
        public Task ReceiveMessage(string message)
        {
            return Task.CompletedTask;
        }
    }
}