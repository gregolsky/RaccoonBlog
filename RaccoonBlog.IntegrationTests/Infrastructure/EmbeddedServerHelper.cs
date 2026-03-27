using Raven.Embedded;

namespace RaccoonBlog.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Provides a single, process-wide initialization of EmbeddedServer.Instance.
    /// Because EmbeddedServer.Instance is a static singleton, calling StartServer()
    /// more than once throws InvalidOperationException. This helper guarantees
    /// exactly-once initialization regardless of which test class runs first.
    /// </summary>
    public static class EmbeddedServerHelper
    {
        private static volatile EmbeddedServer _instance;
        private static readonly object _lock = new object();

        public static EmbeddedServer Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        var server = EmbeddedServer.Instance;
                        server.StartServer();
                        _instance = server;
                    }
                }

                return _instance;
            }
        }
    }
}
