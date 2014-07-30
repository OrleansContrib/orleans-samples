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

using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrainInterfaces;
using System.Diagnostics;
using System.Net;

namespace Grains
{
    public class ManagerGrain : GrainBase, IManagerGrain
    {
        OrleansLogger _logger;
        private List<ISimulatorGrain> _sims = new List<ISimulatorGrain>();
        private IAggregatorGrain _aggregator;
        IOrleansTimer _stattimer, _starttimer;
        int _count;
        string _url;
        double start_lat, start_lon;

        // Counters
        long c_total_requests;
        long c_failed_requests;

        static int REPORT_PERIOD = 10; // seconds

        public Task SetAggregator(IAggregatorGrain aggregator)
        {
            _aggregator = aggregator;
            return TaskDone.Done;
        }

        public override Task ActivateAsync()
        {
            _logger = base.GetLogger("Manager");

            return base.ActivateAsync();
        }

        /// <summary>
        /// Schedule the simulation to start later.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public Task StartSimulators(int delay, int count, string url)
        {
            StartSimulators(delay, count, url, 0, 0);

            return TaskDone.Done;
        }

        public Task StartSimulators(int delay, int count, string url, double latitude, double longitude)
        {
            _count = count;
            _url = url;
            start_lat = latitude;
            start_lon = longitude;
            _starttimer = RegisterTimer(StartSimulatorsDelayed, null, TimeSpan.FromSeconds(delay), TimeSpan.FromDays(1));

            return TaskDone.Done;
        }

        /// <summary>
        /// Instantiate simulator grains and start the simulation on each.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private async Task StartSimulatorsDelayed(object o)
        {
            List<Task> tasks = new List<Task>();

            // Stop the one-time timer
            _starttimer.Dispose();

            long start = this.GetPrimaryKeyLong() * _count;
            for (long i = start; i < start + _count; i++)
            {
                ISimulatorGrain grainRef = SimulatorGrainFactory.GetGrain(i);
                _sims.Add(grainRef);
                tasks.Add(grainRef.StartSimulation(i, _url, this, start_lat, start_lon));
            }

            await Task.WhenAll(tasks);  // wait until all grains have started

            _logger.Info("*** " + _count + " simulators started.");

            _stattimer = RegisterTimer(ReportResults, null, 
                    TimeSpan.FromSeconds(REPORT_PERIOD), TimeSpan.FromSeconds(REPORT_PERIOD));
        }

        /// <summary>
        /// Set the velocity on all simulator grains.
        /// </summary>
        /// <param name="velocity"></param>
        /// <returns></returns>
        public async Task SetVelocity(double velocity)
        {
            List<Task> tasks = new List<Task>();

            _logger.Info("*** start SetVelocity for {0}", this.GetPrimaryKeyLong());

            foreach (var sim in _sims)
            {
                tasks.Add(sim.SetVelocity(velocity));
            }

            await Task.WhenAll(tasks);

            _logger.Info("*** end SetVelocity for {0}", this.GetPrimaryKeyLong());
        }

        /// <summary>
        /// Stop all the simulator grains.
        /// </summary>
        /// <returns></returns>
        public async Task StopSimulators()
        {
            List<Task> tasks = new List<Task>();

            // stop timer
            _stattimer.Dispose();

            // stop simulators
            foreach (var i in _sims)
            {
                tasks.Add(i.StopSimulation());
            }

            await Task.WhenAll(tasks);

            // zero out counters
            c_total_requests = c_failed_requests = 0;

            _logger.Info(_sims.Count + " simulators stopped.");
        }

        /// <summary>
        /// Report stats to the aggregator grain.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public async Task ReportResults(object o)
        {
            _logger.Info("*** manager {0} report results: total={1} failed={2}", this.GetPrimaryKeyLong(), c_total_requests, c_failed_requests);
            
            // send the results back to the aggregator grain
            if (_aggregator != null)
                await _aggregator.AggregateResults(this.GetPrimaryKeyLong(), c_total_requests, c_failed_requests);
            
            // zero out counters
            c_total_requests = c_failed_requests = 0;
        }

        /// <summary>
        /// The simulator grains will call this method to report results.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public Task SendResults(long total_requests, long failed_requests)
        {
            c_total_requests += total_requests;
            c_failed_requests += failed_requests;

            return TaskDone.Done;
        }

    }
}
