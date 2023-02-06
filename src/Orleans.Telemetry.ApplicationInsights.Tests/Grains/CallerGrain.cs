using System.Threading.Tasks;

namespace Orleans.Telemetry.ApplicationInsights.Tests.Grains
{
    public interface ICallerGrain : IGrainWithGuidKey
    {
        Task<string> SendMessageToGrain(string message);
    }

    public class CallerGrain : Grain, ICallerGrain
    {
        public CallerGrain()
        {
            
        }

        public async Task<string> SendMessageToGrain(string message)
        {
            var targetGrain = GrainFactory.GetGrain<ICalledGrain>(5, "keyExt");
            await targetGrain.ReceiveMessage(message);
            return targetGrain.GetGrainId().ToString();
        }
    }
}