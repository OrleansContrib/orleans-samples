//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************

using GPSTracker.Common;
using GPSTracker.GrainInterface;
using Orleans;
using System;
using System.Threading.Tasks;

namespace GPSTracker.GrainImplementation
{
    /// <summary>
    /// Orleans grain implementation class.
    /// </summary>
    [Reentrant]
    public class DeviceGrain : Orleans.GrainBase, IDeviceGrain
    {
        private DeviceMessage lastMessage { get; set; }
        public Task<DeviceMessage> LastMessage
        {
            get { return Task.FromResult(lastMessage); }
        }

        private string description { get; set; }
        public Task<string> Description
        {
            get { return Task.FromResult(description); }
        }

        public async Task ProcessMessage(DeviceMessage message)
        {
            if (null == this.lastMessage || this.lastMessage.Latitude != message.Latitude || this.lastMessage.Longitude != message.Longitude)
            {
                // only sent a notification if the position has changed
                var notifier = PushNotifierGrainFactory.GetGrain(0);
                var speed = GetSpeed(this.lastMessage, message);

                // record the last message
                this.lastMessage = message;

                // forward the message to the notifier grain
                var statusMessage = new DeviceStatusMessage(message, speed, description);
                await notifier.SendMessage(statusMessage);
            }
            else
            {
                // the position has not changed, just record the last message
                this.lastMessage = message;
            }
        }

        public async Task Register(Device device)
        {
            description = device.Description;
        }

        static double GetSpeed(DeviceMessage message1, DeviceMessage message2)
        {
            // calculate the speed of the device, using the interal state of the grain
            if (message1 == null) return 0;
            if (message2 == null) return 0;

            const double R = 6371 * 1000;
            var x = (message2.Longitude - message1.Longitude) * Math.Cos((message2.Latitude + message1.Latitude) / 2);
            var y = message2.Latitude - message1.Latitude;
            var distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) * R;
            var time = (message2.Timestamp - message1.Timestamp).TotalSeconds;
            if (time == 0) return 0;
            return distance / time;
        }
    }
}
