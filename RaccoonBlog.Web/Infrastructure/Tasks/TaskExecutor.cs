using System;
using System.Threading.Tasks;
using Raven.Client.Documents;

namespace HibernatingRhinos.Loci.Common.Tasks
{
    public static class TaskExecutor
    {
        public static IDocumentStore DocumentStore
        {
            get { return _documentStore; }
            set
            {
                if (_documentStore == null)
                {
                    _documentStore = value;
                }
            }
        }
        private static IDocumentStore _documentStore;

        public static Action<Exception> ExceptionHandler { get; set; }

        public static void ExcuteLater(BackgroundTask task)
        {
            Task.Run(() =>
            {
                try
                {
                    ExecuteTask(task);
                }
                catch (Exception ex)
                {
                    ExceptionHandler?.Invoke(ex);
                }
            });
        }

        public static void ExecuteTask(BackgroundTask task)
        {
            for (var i = 0; i < 10; i++)
            {
                using (var session = _documentStore.OpenSession())
                {
                    switch (task.Run(session))
                    {
                        case true:
                        case false:
                            return;
                        case null:
                            break;
                    }
                }
            }
        }
    }
}