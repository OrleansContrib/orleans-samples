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

using GrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Grains
{
    public class AggregatorGrain : GrainBase, IAggregatorGrain
    {
        private ISimulationObserver _observer;
        OrleansLogger _logger;
        private Stopwatch _sw;
        IOrleansTimer _stattimer;

        static int REPORT_PERIOD = 15; // seconds

        // Counters
        private long c_total_requests, c_failed_requests;
        Dictionary<long, long> all_total_requests = new Dictionary<long, long>();
        Dictionary<long, long> all_failed_requests = new Dictionary<long, long>();


        // note where observer notifications are to be delivered to 
        public Task SetObserver(ISimulationObserver observer)
        {
            _observer = observer;
            return TaskDone.Done;
        }


        // report results as notication 
        public Task ReportResults(object o)
        {
            _logger.Info("*** aggregator report results: " + c_total_requests + " " + c_failed_requests);
            
            // Send results to Observer
            _observer.ReportResults(_sw.ElapsedMilliseconds, c_total_requests, c_failed_requests, all_total_requests, all_failed_requests);
            
            return TaskDone.Done;
        }



        public override Task ActivateAsync()
        {
            _logger = base.GetLogger("Aggregator");

            _sw = new Stopwatch();
            _sw.Start();

            _stattimer = RegisterTimer(ReportResults, null, 
                    TimeSpan.FromSeconds(REPORT_PERIOD), TimeSpan.FromSeconds(REPORT_PERIOD));

            return base.ActivateAsync();
        }



        public Task AggregateResults(long id, long total_requests, long failed_requests)
        {
            // Simple aggregations examples
            c_total_requests += total_requests;
            c_failed_requests += failed_requests;

            // Counters per manager id
            if (!all_total_requests.ContainsKey(id))
                all_total_requests.Add(id, 0);

            if (!all_failed_requests.ContainsKey(id))
                all_failed_requests.Add(id, 0);
            
            all_total_requests[id] += total_requests;
            all_failed_requests[id] += failed_requests;

            return TaskDone.Done;
        }
    }
}
