using GPSTracker.Common;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using System.Linq;

namespace GPSTracker.Web.Controllers
{
    public class LocationHub : Hub
    {
        public void LocationUpdate(DeviceStatusMessage message)
        {
            // Forward a single messages to all clients
            Clients.Group("BROWSERS").locationUpdate(message);
            Clients.Group("DEVICES").locationUpdate(message);
        }

        public void LocationUpdates(DeviceStatusBatch messages)
        {
            // Forward a batch of messages to all browsers
            Clients.Group("BROWSERS").locationUpdates(messages);
            
            //Forward only a subset of messages to Apps, otherwise the device is flooded and might not be able to handle the load
            var batchForDevices = new List<DeviceStatusMessage>();
            batchForDevices.Add(messages.Messages[0]);
            foreach (var message in messages.Messages.Where(m => !string.IsNullOrWhiteSpace(m.Description)))
                batchForDevices.Add(message);
            Clients.Group("DEVICES").locationUpdates(new DeviceStatusBatch { Messages = batchForDevices.ToArray() });
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            if (Context.Headers.Get("ORLEANS") != "GRAIN")
            {

                if (Context.Headers.Get("ORLEANS") == "DEVICES")
                    // This is an App therefore add this connection to the browser group
                    Groups.Add(Context.ConnectionId, "DEVICES");
               
                else
                    // This connection does not have the GRAIN header, so it must be a browser
                    // Therefore add this connection to the browser group
                    Groups.Add(Context.ConnectionId, "BROWSERS");
            }
          
            return base.OnConnected();
        }


    }
}