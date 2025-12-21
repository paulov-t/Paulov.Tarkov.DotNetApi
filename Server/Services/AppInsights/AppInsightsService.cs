using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Reflection;
using System.Text;

namespace Paulov.Tarkov.AppInsights
{
    public class AppInsightsService : IAppInsightsService
    {
        private IConfiguration configuration;
        TelemetryClient telemetryClient;

        // The actual logging instance used to write entries
        private IOperationHolder<RequestTelemetry> AppRunTelemetry;

        public AppInsightsService(IConfiguration configuration)
        {
            this.configuration = configuration;

            var telemetryConfiguration = new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration()
            {
                ConnectionString = configuration.GetValue<string>("APPLICATIONINSIGHTS_CONNECTION_STRING"),
                InstrumentationKey = configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY"),
            };

            telemetryClient = new TelemetryClient(telemetryConfiguration);

            var userId = GetUserId();

            telemetryClient.Context.User.Id = userId.ToString();
            telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();

            var appVersion = Assembly.GetAssembly(typeof(AppInsightsService)).GetName().Version.ToString();
            telemetryClient.Context.Component.Version = appVersion;

            AppRunTelemetry = telemetryClient.StartOperation<RequestTelemetry>($"{appVersion}");
            AppRunTelemetry.Telemetry.Start();
        }

        private Guid GetUserId()
        {
            var userId = Guid.NewGuid();

            try
            {

                string userName = Environment.MachineName;
                if (userName.Length < 16)
                {
                    while (userName.Length < 16)
                    {
                        userName += " ";
                    }
                }
                else if (userName.Length > 16)
                {
                    userName = userName.Substring(0, 16);
                }
                string base64UserName = Convert.ToHexString(Encoding.UTF8.GetBytes(userName));
                userId = new Guid(base64UserName);
            }
            catch
            {

            }
            finally
            {

            }

            return userId;

        }

        public void Dispose()
        {
            AppRunTelemetry.Telemetry.Stop();
        }

        public void TrackEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            telemetryClient.TrackEvent(eventName, properties, metrics);
        }

        public void TrackEvent(string eventName)
        {
            telemetryClient.TrackEvent(eventName);
        }

        public void TrackException(Exception ex, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            telemetryClient.TrackException(ex, properties, metrics);
        }

        public void TrackException(Exception ex)
        {
            telemetryClient.TrackException(ex);
        }

        public void TrackPageView(string pageView)
        {
            if (Uri.TryCreate(pageView, UriKind.Relative, out var url))
                telemetryClient.TrackPageView(new PageViewTelemetry(pageView) { Url = url, Timestamp = DateTimeOffset.Now });
            else
                telemetryClient.TrackPageView(new PageViewTelemetry(pageView));
        }
    }
}
