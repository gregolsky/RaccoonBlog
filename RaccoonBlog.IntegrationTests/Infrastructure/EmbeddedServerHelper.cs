using Raven.Embedded;

namespace RaccoonBlog.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Provides a single, process-wide initialization of the RavenDB EmbeddedServer.
    /// Both TestWebApplicationFactory and PostSchedulingStrategyTests share this helper
    /// to avoid calling StartServer() more than once (which throws InvalidOperationException).
    /// </summary>
    internal static class EmbeddedServerHelper
    {
        private static EmbeddedServer _instance;
        private static readonly object _lock = new object();

        public static EmbeddedServer Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = EmbeddedServer.Instance;
                        _instance.StartServer();
                    }
                    return _instance;
                }
            }
        }
    }
}
