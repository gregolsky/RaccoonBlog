using System;
using NLog;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions;

namespace HibernatingRhinos.Loci.Common.Tasks
{
	public abstract class BackgroundTask
	{
		protected IDocumentSession DocumentSession;

		private readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected virtual void Initialize(IDocumentSession session)
		{
			DocumentSession = session;
			DocumentSession.Advanced.UseOptimisticConcurrency = true;
		}

		protected virtual void OnError(Exception e)
		{
		}

		public bool? Run(IDocumentSession openSession)
		{
			Initialize(openSession);
			try
			{
				Execute();
				DocumentSession.SaveChanges();
				return true;
			}
			catch (ConcurrencyException e)
			{
				logger.ErrorException("Could not execute task " + GetType().Name, e);
				OnError(e);
				return null;
			}
			catch (Exception e)
			{
				logger.ErrorException("Could not execute task " + GetType().Name, e);
				OnError(e);
				return false;
			}
		}

		public abstract void Execute();
	}
}
