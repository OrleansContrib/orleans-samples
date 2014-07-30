using GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimulatorUI
{
    public class SimulationObserver : ISimulationObserver
    {
        public long c_sent;
        public long c_errors;
        public Dictionary<long, long> c_sent_requests = new Dictionary<long,long>();
        public Dictionary<long, long> c_failed_requests = new Dictionary<long,long>();

        public void ReportResults(long millis, long sent, long errors, Dictionary<long, long> all_sent, Dictionary<long, long> all_errors)
        {
            var avg = sent / (millis / 1000);

            c_sent = sent;
            c_errors = errors;
            c_sent_requests = all_sent;
            c_failed_requests = all_errors;
        }
    }
}