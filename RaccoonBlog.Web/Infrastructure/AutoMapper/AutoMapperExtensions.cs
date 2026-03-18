using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace RaccoonBlog.Web.Infrastructure.AutoMapper
{
	public static class AutoMapperExtensions
	{
		// Service locator for IMapper - set during application startup
		private static IMapper _mapper;

		public static void Initialize(IMapper mapper)
		{
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		private static IMapper Mapper
		{
			get
			{
				if (_mapper == null)
					throw new InvalidOperationException("AutoMapper has not been initialized. Call AutoMapperExtensions.Initialize(mapper) during application startup.");
				return _mapper;
			}
		}

		public static List<TResult> MapTo<TResult>(this IEnumerable self)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			// AutoMapper 12.x: Use the simple generic Map<T> method
			// AutoMapper will automatically handle the collection mapping
			try
			{
				// Try direct mapping first - AutoMapper can infer source type from the collection
				return Mapper.Map<List<TResult>>(self);
			}
			catch (AutoMapperMappingException)
			{
				// If direct mapping fails, fallback to mapping items individually
				// This handles cases where the mapping profile might not be configured for collections
				var result = new List<TResult>();
				foreach (var item in self)
				{
					if (item != null)
					{
						var mapped = Mapper.Map<TResult>(item);
						result.Add(mapped);
					}
				}
				return result;
			}
		}

		public static TResult MapTo<TResult>(this object self)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			return Mapper.Map<TResult>(self);
		}

		public static TResult MapPropertiesToInstance<TResult>(this object self, TResult value)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			return Mapper.Map(self, value);
		}
	}
}