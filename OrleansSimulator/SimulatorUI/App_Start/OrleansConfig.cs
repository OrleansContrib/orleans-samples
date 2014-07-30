using GrainInterfaces;
using Orleans.Host.Azure.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimulatorUI
{
    public class OrleansConfig
    {
        public static async void InitializeOrleans()
        {
            // Create the Observer object
            MvcApplication.GlobalObserver = new SimulationObserver();

            // Get a reference to the aggregator grain
            IAggregatorGrain aggregator = AggregatorGrainFactory.GetGrain(0);

            // Set observer on the aggregator grain
            ISimulationObserver observer = await SimulationObserverFactory.CreateObjectReference(MvcApplication.GlobalObserver);  // convert our class into a grain reference
            await aggregator.SetObserver(observer);  // then set ourselves up to receive notifications on ReportResults()
        }
    }
}