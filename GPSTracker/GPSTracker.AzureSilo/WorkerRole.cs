using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Orleans.Host.Azure;

namespace GPSTracker.AzureSilo
{
    public class WorkerRole : RoleEntryPoint
    {
        private OrleansAzureSilo orleansAzureSilo;

        protected static bool collectPerfCounters = false;
        protected static bool collectWindowsEventLogs = false;
        protected static bool fullCrashDumps = false;

        public WorkerRole()
        {
            Console.WriteLine("OrleansAzureSilos-Constructor called");
        }

        public override bool OnStart()
        {
            Trace.WriteLine("OrleansAzureSilos-OnStart called", "Information");

            Trace.WriteLine("OrleansAzureSilos-OnStart Initializing config", "Information");

            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += RoleEnvironmentChanging;

            Trace.WriteLine("OrleansAzureSilos-OnStart Initializing diagnostics", "Information");

            DiagnosticMonitorConfiguration diagConfig = ConfigureDiagnostics();

            // Start the diagnostic monitor. 
            // The parameter references a connection string specified in the service configuration file 
            // that indicates the storage account where diagnostic information will be transferred. 
            // If the value of this setting is "UseDevelopmentStorage=true" then logs are written to development storage.
            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", diagConfig);

            Trace.WriteLine("OrleansAzureSilos-OnStart Starting Orleans silo", "Information");

            orleansAzureSilo = new OrleansAzureSilo();

            bool ok = base.OnStart();
            if (ok)
            {
                ok = orleansAzureSilo.Start(RoleEnvironment.DeploymentId, RoleEnvironment.CurrentRoleInstance);
            }
            Trace.WriteLine("OrleansAzureSilos-OnStart Orleans silo started ok=" + ok, "Information");
            return ok;
        }

        public override void Run()
        {
            Trace.WriteLine("OrleansAzureSilos-Run entry point called", "Information");
            orleansAzureSilo.Run(); // Call will block until silo is shutdown
        }

        public override void OnStop()
        {
            Trace.WriteLine("OrleansAzureSilos-OnStop called", "Information");
            orleansAzureSilo.Stop();
            RoleEnvironment.Changing -= RoleEnvironmentChanging;
            base.OnStop();
            Trace.WriteLine("OrleansAzureSilos-OnStop finished", "Information");
        }

        public static DiagnosticMonitorConfiguration ConfigureDiagnostics()
        {
            // Get default initial configuration.
            DiagnosticMonitorConfiguration diagConfig = DiagnosticMonitor.GetDefaultInitialConfiguration();

            // Add performance counters to the diagnostic configuration
            if (collectPerfCounters)
            {
                diagConfig.PerformanceCounters.DataSources.Add(
                    new PerformanceCounterConfiguration
                    {
                        CounterSpecifier = @"\Processor(_Total)\% Processor Time",
                        SampleRate = TimeSpan.FromSeconds(5)
                    });
                diagConfig.PerformanceCounters.DataSources.Add(
                    new PerformanceCounterConfiguration
                    {
                        CounterSpecifier = @"\Memory\Available Mbytes",
                        SampleRate = TimeSpan.FromSeconds(5)
                    });
            }

            // Add event collection from the Windows Event Log
            if (collectWindowsEventLogs)
            {
                diagConfig.WindowsEventLog.DataSources.Add("System!*");
                diagConfig.WindowsEventLog.DataSources.Add("Application!*");
            }

            // Schedule log transfers into storage
            //diagConfig.DiagnosticInfrastructureLogs.ScheduledTransferLogLevelFilter = LogLevel.Error;
            diagConfig.DiagnosticInfrastructureLogs.ScheduledTransferLogLevelFilter = LogLevel.Information;
            diagConfig.DiagnosticInfrastructureLogs.ScheduledTransferPeriod = TimeSpan.FromMinutes(5);

            // Specify whether full crash dumps should be captured 
            Microsoft.WindowsAzure.Diagnostics.CrashDumps.EnableCollection(fullCrashDumps);

            return diagConfig;
        }


        private static void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            int i = 1;
            foreach (var c in e.Changes)
            {
                Trace.WriteLine(string.Format("RoleEnvironmentChanging: #{0} Type={1} Change={2}", i++, c.GetType().FullName, c));
            }

            // If a configuration setting is changing);
            if (e.Changes.Any((RoleEnvironmentChange change) => change is RoleEnvironmentConfigurationSettingChange))
            {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }
    }
}
