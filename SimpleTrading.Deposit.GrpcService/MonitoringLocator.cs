using Prometheus;

namespace SimpleTrading.Deposit.GrpcService
{
    public class MonitoringLocator
    {
        public static Summary RequestDurationSummary = GetRequestDurationSummary();
        
        private static Summary GetRequestDurationSummary()
        {
            var config = new SummaryConfiguration
            {
                LabelNames = new[] {"method"}
            };

            return Metrics.CreateSummary("deposit_api_request_duration", "Average request duration", config);
        }
    }
}