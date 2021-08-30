using System.Threading.Tasks;

namespace Orleans.Telemetry.ApplicationInsights.Tests.Grains
{
    public interface ICallerGrain : IGrainWithGuidKey
    {
        Task SendMessageToGrain(string message);
    }

    public class CallerGrain : Grain, ICallerGrain
    {
        public async Task SendMessageToGrain(string message)
        {
            await GrainFactory.GetGrain<ICalledGrain>(5, "keyExt").ReceiveMessage(message);
        }
    }
}