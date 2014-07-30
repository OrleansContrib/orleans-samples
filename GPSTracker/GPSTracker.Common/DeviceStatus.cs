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

namespace GPSTracker.Common
{
    public class DeviceStatusMessage : DeviceMessage
    {

        public DeviceStatusMessage()
        { }

        public DeviceStatusMessage(DeviceMessage deviceMessage, double velocity, string description)
        {
            this.Latitude = deviceMessage.Latitude;
            this.Longitude = deviceMessage.Longitude;
            this.MessageId = deviceMessage.MessageId;
            this.DeviceId = deviceMessage.DeviceId;
            this.Timestamp = deviceMessage.Timestamp;
            this.Velocity = velocity;
            this.Description = description;
        }

        public double Velocity { get; set; }
        public string Description { get; set; }
    }

    public class DeviceStatusBatch
    {
        public DeviceStatusMessage[] Messages;
    }
}
