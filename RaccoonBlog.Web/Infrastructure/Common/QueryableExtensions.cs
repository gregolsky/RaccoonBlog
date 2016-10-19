using System;
using System.Linq;
using RaccoonBlog.Web.Models;
using Raven.Client.Linq;

namespace RaccoonBlog.Web.Infrastructure.Common
{
	public static class QueryableExtensions
	{
		public static IQueryable<T> Paging<T>(this IQueryable<T> query, int currentPage, int defaultPage, int pageSize)
		{
			return query
				.Skip((currentPage - defaultPage)*pageSize)
				.Take(pageSize);
		}
	}
}