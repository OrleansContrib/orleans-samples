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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using Orleans;
using GrainInterfaces;
using GPSTracker.Common;
using System.IO;
using System.Net.Http;

namespace Grains
{
    /// <summary>
    /// Orleans grain implementation class.
    /// </summary>
    public class SimulatorGrain : GrainBase, ISimulatorGrain
    {
        OrleansLogger _logger;
        IManagerGrain _manager;
        IOrleansTimer _reqtimer, _stattimer;
        string _url;
        HttpClient client = new HttpClient();

        // State
        double cur_lat = 0, cur_long = 0;
        Guid device_id;
        double lat_speed, long_speed;
        double speed_factor = 0.25;

        // Counters
        long c_total_requests;
        long c_failed_requests;

        static int MAX_DELAY = 2; // seconds
        static int PERIOD = 1; // seconds
        static int REPORT_PERIOD = 5; // seconds

        /// <summary>
        /// Set the velocity factor
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
        public Task SetVelocity(double velocity)
        {
            this.speed_factor = velocity;

            return TaskDone.Done;
        }

        /// <summary>
        /// Grain activation.
        /// </summary>
        /// <returns></returns>
        public override Task ActivateAsync()
        {
            _logger = base.GetLogger("Simulator");

            return base.ActivateAsync();
        }

        /// <summary>
        /// Start the simulation.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        public Task StartSimulation(long id, string url, IManagerGrain manager)
        {
            return StartSimulation(id, url, manager, 0, 0);
        }

        /// <summary>
        /// Start simulation at a given latitude/longitude coordinate.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="url"></param>
        /// <param name="managerGrain"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public Task StartSimulation(long id, string url, IManagerGrain managerGrain, double latitude, double longitude)
        {
            _url = url;
            _manager = managerGrain;

            var rand = new Random((int)this.GetPrimaryKeyLong());

            // initialize simulation parameters
            cur_lat = latitude + (rand.NextDouble() - 0.5) / 10.0;
            cur_long = longitude + (rand.NextDouble() - 0.5) / 10.0;
            device_id = Guid.NewGuid();
            lat_speed = (rand.NextDouble() - 0.5) / 10.0;
            long_speed = (rand.NextDouble() - 0.5) / 10.0;

            _logger.Info("*** simulator " + this.GetPrimaryKeyLong() + " starting " + cur_lat + " " + cur_long + " " + device_id);

            // start the timers
            _reqtimer = RegisterTimer(SendRequest, null,
                    TimeSpan.FromSeconds(rand.Next(MAX_DELAY)), TimeSpan.FromSeconds(PERIOD));
            _stattimer = RegisterTimer(ReportResults, null,
                    TimeSpan.FromSeconds(REPORT_PERIOD), TimeSpan.FromSeconds(REPORT_PERIOD));

            return TaskDone.Done;
        }

        /// <summary>
        /// Stop the simulation by disposing of the timers.
        /// </summary>
        /// <returns></returns>
        public Task StopSimulation()
        {
            _reqtimer.Dispose();
            _stattimer.Dispose();

            // reset counters
            c_total_requests = c_failed_requests = 0;

            return TaskDone.Done;
        }

        /// <summary>
        /// Send an asynchronous request and await the response.
        /// </summary>
        /// <param name="o"></param>
        public async Task SendRequest(object o)
        {
            // Update grain state

            cur_lat += lat_speed * speed_factor;
            cur_long += long_speed * speed_factor;

            if (cur_lat > 90 || cur_lat < -90)
                lat_speed = -lat_speed;

            if (cur_long > 180 || cur_long < -180)
                long_speed = -long_speed;

            // Send the request

            try
            {
                // Compute the device message
                DeviceMessage msg = new DeviceMessage(cur_lat, cur_long, 0, device_id, DateTime.Now);

                //_logger.Info("*** {0}-{1} sending request {2} {3}", _manager.GetPrimaryKeyLong(), this.GetPrimaryKeyLong(), cur_lat, cur_long);

                // Make the HTTP request 
                HttpResponseMessage response = await client.PostAsJsonAsync<DeviceMessage>(_url, msg);

                // Check for error codes
                if (!response.IsSuccessStatusCode)
                {
                    //_logger.Info(0, "*** HTTP status: {0}", response.StatusCode);
                    ++c_failed_requests;
                }

                // Count the response 
                ++c_total_requests;
            }
            catch (Exception e)
            {
                _logger.Error(0, "*** HttpClient Exception: ", e);
            }
        }

        /// <summary>
        /// Periodically report results to the manager grain.
        /// </summary>
        /// <param name="o"></param>
        public async Task ReportResults(object o)
        {
            await _manager.SendResults(c_total_requests, c_failed_requests);
        }

    }
}
