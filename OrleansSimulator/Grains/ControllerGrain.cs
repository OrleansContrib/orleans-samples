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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grains
{
    public class ControllerGrain : GrainBase, IControllerGrain
    {
        IList<IManagerGrain> all_managers = new List<IManagerGrain>();

        public async Task StartSimulation(int batch_count, int batch_size, int delay, int runtime, string url)
        {
            // List of cities with coordinates
            var cities = new CityCoordinates();

            // create an aggregator grain to track results from the load test
            IAggregatorGrain aggregator = AggregatorGrainFactory.GetGrain(0);

            // Instantiate the manager grains and start the simulations
            
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < batch_count; i++)
            {
                Console.WriteLine("Starting batch #{0}", i + 1);
                IManagerGrain manager = ManagerGrainFactory.GetGrain(i);

                all_managers.Add(manager);

                var city = cities.RandomCity();
                tasks.Add(manager.SetAggregator(aggregator)); // link in the aggregator
                tasks.Add(manager.StartSimulators(i * delay, batch_size, url, city.Latitude, city.Longitude));  // start the simulation
            }

            await Task.WhenAll(tasks);
        }

        public async Task StopSimulation()
        {
            List<Task> tasks = new List<Task>();

            foreach (var manager in all_managers)
            {
                tasks.Add(manager.StopSimulators());
            }

            await Task.WhenAll(tasks);
        }

        public async Task SetVelocity(double velocity)
        {
            List<Task> tasks = new List<Task>();

            foreach (var manager in all_managers)
            {
                tasks.Add(manager.SetVelocity(velocity));
            }

            await Task.WhenAll(tasks);
        }
    }
}
