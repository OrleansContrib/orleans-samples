using GPSTracker.Common;
using GPSTracker.GrainInterface;
using System.Threading.Tasks;
using System.Web.Http;

namespace GPSTracker.WebAPI.Controllers
{
    public class DevicesController : ApiController
    {
        public Task ProcessMessage([FromBody]DeviceMessage message)
        {
            // send the message to Orleans
            var deviceGrain = DeviceGrainFactory.GetGrain(message.DeviceId);
            return deviceGrain.ProcessMessage(message);        
        }

        public Task Register([FromBody]Device device)
        {
            var deviceGrain = DeviceGrainFactory.GetGrain(device.DeviceId);
            return deviceGrain.Register(device);
        }
    }
}