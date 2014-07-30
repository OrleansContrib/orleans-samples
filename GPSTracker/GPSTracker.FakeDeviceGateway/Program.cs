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
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GPSTracker.FakeDeviceGateway
{
    class Program
    {
        static string baseURL = "http://xxxxxxxxx.cloudapp.net:8080/api/devices/processmessage"; // Azure WebAPI URL
        //static string baseURL = "http://localhost:8080/api/devices/processmessage"; //local WebAPI URL
        static int counter = 0;
        static Random rand = new Random();

        // San Francisco (37.75, -122.45): approximate boundaries.
        const double SFLatMin = 37.708;
        const double SFLatMax = 37.78;
        const double SFLonMin = -122.50;
        const double SFLonMax = -122.39;

        // Utrecht (52.09, 5.12): approximate boundaries
        //const double SFLatMin = 51.95;
        //const double SFLatMax = 52.35;
        //const double SFLonMin = 4.8;
        //const double SFLonMax = 5.2;

        static void Main(string[] args)
        {
            // simulate 20 devices
            var devices = new List<Device>();
            for (var i = 0; i < 20; i++)
            {
                devices.Add(new Device()
                {
                    DeviceId = Guid.NewGuid(),
                    Lat = rand.NextDouble(SFLatMin, SFLatMax),
                    Lon = rand.NextDouble(SFLonMin, SFLonMax),
                    Direction = rand.NextDouble(-Math.PI, Math.PI),
                    Speed = rand.NextDouble(0, 0.0005)
                });
            }

            var timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += (s, e) =>
            {
                Console.Write(". ");
                Interlocked.Exchange(ref counter, 0);
            };
            timer.Start();

            // create a thread for each device, and continually move it's position
            foreach (var device in devices)
            {
                var ts = new ThreadStart(() =>
                {
                    while (true)
                    {
                        try
                        {
                            SendMessage(device).Wait();
                            Thread.Sleep(rand.Next(500, 2500));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                    }
                });
                new Thread(ts).Start();
            }
        }

        private static async Task SendMessage(Device device)
        {
            // simulate the device moving
            device.Speed += rand.NextDouble(-0.0001, 0.0001);
            device.Direction += rand.NextDouble(-0.001, 0.001);

            var lastLat = device.Lat;
            var lastLon = device.Lon;

            UpdateDevicePosition(device);

            if (lastLat == device.Lat || lastLon == device.Lon)
            {
                // the device has hit the boundary, so reverse it's direction
                device.Speed = -device.Speed;
                UpdateDevicePosition(device);
            }

            // send the message to WebAPI
            DeviceMessage message = new DeviceMessage(device.Lat, device.Lon, counter, device.DeviceId, DateTime.UtcNow);

            HttpClient webAPIClient = new HttpClient();
            HttpResponseMessage responseMessage = await webAPIClient.PostAsJsonAsync<DeviceMessage>(baseURL, message);

            Interlocked.Increment(ref counter);
        }

        private static void UpdateDevicePosition(Device device)
        {
            device.Lat += Math.Cos(device.Direction) * device.Speed;
            device.Lon += Math.Sin(device.Direction) * device.Speed;
            device.Lat = device.Lat.Cap(SFLatMin, SFLatMax);
            device.Lon = device.Lon.Cap(SFLonMin, SFLonMax);
        }

    }
}
