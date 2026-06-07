namespace Paulov.Tarkov.AppInsights
{
    public interface IAppInsightsService : IDisposable
    {
        public void TrackPageView(string pageView);

        public void TrackEvent(string eventName);
        public void TrackEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null);

        public void TrackException(Exception ex);
        public void TrackException(Exception ex, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null);
    }
}
