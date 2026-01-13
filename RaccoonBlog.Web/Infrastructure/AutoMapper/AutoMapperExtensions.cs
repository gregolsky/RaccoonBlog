using System;
using System.Collections;
using System.Collections.Generic;
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

			return (List<TResult>) Mapper.Map(self, self.GetType(), typeof (List<TResult>));
		}

		public static TResult MapTo<TResult>(this object self)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			return (TResult) Mapper.Map(self, self.GetType(), typeof (TResult));
		}

		public static TResult MapPropertiesToInstance<TResult>(this object self, TResult value)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			return (TResult) Mapper.Map(self, value, self.GetType(), typeof (TResult));
		}
	}
}