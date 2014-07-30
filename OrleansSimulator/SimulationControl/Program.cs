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
using System.Text;
using System.Threading.Tasks;
using Orleans;
using GrainInterfaces;
using System.Threading;

namespace SimulationControl
{
    // the load simulator controller 
    // this class also implments the implements the observer derived interface ISimulationObserver 
    public class SimulationController : ISimulationObserver
    {
        ISimulationObserver observer;
        List<IManagerGrain> managers = new List<IManagerGrain>();

        const int BATCH_COUNT = 10;
        const int BATCH_SIZE = 100;
        const int DELAY_STEPS = 15; // seconds
        const int RUN_TIME = 300; // seconds
        const string URL = "http://devicetracker.cloudapp.net:8080/api/devices/processmessage";  // Change to point to the web site under test

        /// <summary>
        /// Start the simulation via the controller grain.
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            OrleansClient.Initialize();

            // create an aggregator grain to track results from the load test
            IAggregatorGrain aggregator = AggregatorGrainFactory.GetGrain(0);

            // set this SimulationController class as an observer on the aggregator grain
            observer = await SimulationObserverFactory.CreateObjectReference(this);  // convert our class into a grain reference
            await aggregator.SetObserver(observer);  // then set ourselves up to receive notifications on ReportResults()

            // Instantiate the manager grains and start the simulations
            // Pause between each batch to ramp up load gradually
            for (int i = 0;  i < BATCH_COUNT;  i++)
            {
                Console.WriteLine("Starting batch #{0}", i + 1);
                IManagerGrain manager = ManagerGrainFactory.GetGrain(i);
                managers.Add(manager);  // store grain reference 

                await manager.SetAggregator(aggregator); // link in the aggregator
                await manager.StartSimulators(i*DELAY_STEPS*1000, BATCH_SIZE, URL);  // start the sinulation
            }

            // Sleep for the duration of the test
            Console.WriteLine("Running test...");
            Thread.Sleep(RUN_TIME * 1000);  // low value just for test

            // Gradually stop simulators
            foreach (var i in managers)
            {
                Console.WriteLine("Stopping step #{0}", managers.IndexOf(i) + 1);
                await i.StopSimulators();
                Thread.Sleep(DELAY_STEPS * 1000);
            }
        }

        /// <summary>
        /// This method is called by the aggregator grain to report aggregated results.
        /// </summary>
        /// <param name="millis"></param>
        /// <param name="sent"></param>
        /// <param name="errors"></param>
        /// <param name="size"></param>
        public void ReportResults(long millis, long sent, long errors, Dictionary<long, long> all_sent, Dictionary<long, long> all_errors)
        {
            var avg = sent / (millis / 1000);
            Console.WriteLine("avg req/s: {0} sent: {2} errors: {3}", avg, millis, sent, errors);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var prog = new SimulationController();
            prog.Run().Wait();

            Console.WriteLine("--> Press any key to exit program <--");
            Console.ReadKey();

            Environment.Exit(0);
        }
    }
}
