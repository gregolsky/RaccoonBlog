using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using FluentScheduler;
using HibernatingRhinos.Loci.Common.Controllers;
using Raven.Client;

namespace RaccoonBlog.Web.Infrastructure.Jobs
{
    public abstract class JobBase : IJob, IRegisteredObject
    {
        private readonly object _lock = new object();

        private bool _shuttingDown;

        protected readonly IDocumentStore DocumentStore;

        protected JobBase()
        {
            DocumentStore = RavenController.DocumentStore;
            HostingEnvironment.RegisterObject(this);
        }

        protected abstract void Run(IDocumentSession session);

        public void Execute()
        {
            lock (_lock)
            {
                if (_shuttingDown)
                    return;

                using (var session = DocumentStore.OpenSession())
                {
                    Run(session);
                    session.SaveChanges();
                }
            }
        }

        public void Stop(bool immediate)
        {
            lock (_lock)
            {
                _shuttingDown = true;
            }

            HostingEnvironment.UnregisterObject(this);
        }
    }
}