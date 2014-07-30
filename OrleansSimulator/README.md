## Orleans Load Simulator Sample

This sample illustrates how Orleans can used to generate test load against a system or service. Whilst there are many web and load testing tools, including Visual Studio Test Manager, this scenario focuses on scenarios where a smart client is needed, cases where the load logic needs to be expressed in code, for example, the data needs to be calculated on the fly, or derived from response data, or to have certain variances introduced at runtime, or to use non web protocols to communicate, etc. In these cases, the logic can be expressed simply and concisely in a grain, and Orleans can be used to create, distribute and manage the load test.  

### Architecture 

A console application is used to drive this sample. It creates a number of Manager grains, which are responsible for managing a “batch” of load tests. This model is intended to simplify the management and aggregation of large number of load grains, by assigning them into smaller batches.

```cs
public interface IManagerGrain : IGrain 
{
	Task SetAggregator(IAggregatorGrain aggregator);
	Task StartSimulators(string url);
	Task StopSimulators();
	Task SendResults(List<HttpWebResponse> results);  
}
```

The console program calls the StartSimulators function to initiate the load test, passing in the url to be tested, then calls StopSimulators at the end of the test period.  

Inside the Manager grain, the StartSimulators function starts a small batch of Simulator grains, which do the test workload. 

```cs
public interface ISimulatorGrain : Orleans.IGrain  
{
	Task StartSimulation(long id, string url, IManagerGrain managerGrain);
	Task StopSimulation(); 
}
```

The Simulator grain uses the Orleans TransientTime functionality to set up two timers, one to send load data to the target url, via the SendRequest function, and the other to send the HttpWebResponses from the grain to the manager. 

In order to gather the test results, prior to starting the test run, the console program must also create an aggregator grain, and then call the SetAggregator function on the Manager grain, passing in  a reference to the aggregator grain.

```cs
public interface IAggregatorGrain : IGrain 
{
	Task SetObserver(ISimulationObserver observer);
	Task AggregateResults(List<HttpWebResponse> results); 
}
```

The SimulationController class in the console application implements the observer derived ISimulationObserver interface, which allows it to receive notifications from the aggregator.

```cs
public interface ISimulationObserver : IGrainObserver 
{
	void ReportResults(long millis, int sent, int errors, long size); 
}
```

It sets this up by calling a static method on the Orleans observer factory, CreateObjectReference(), to turn itself into a grain reference, which can then be passed to the subscription method on the aggregator grain.  The ReportResults() method that gets called simply outputs the results to the console.

In this sample, the actual workload logic simply pings a website, and tracks the http result, but in real life, this is where the client logic would be implemented, in the SendRequest() function in the Simulator grain. In our model, each Simulator grain represents a virtual user.

## Running the sample

In Program.cs, change the following parameters:

- `BATCH_SIZE` : size of an individual batch of simulators, e.g. 100;
- `BATCH_COUNT` : how many groups of BATCH_SIZE simulators to start in parallel, e.g. 5
- `DELAY_STEPS` : how long to wait between groups when starting or stopping, e.g. 30 seconds
- `RUN_TIME` : how long to run the test itself, e.g. 300 seconds
- `URL` : the URL to test, e.g. "http://myapplication.cloudapp.net/"

Each Simulator will represent a virtual user, sending out one HTTP request per second by default, using Orleans timers.
