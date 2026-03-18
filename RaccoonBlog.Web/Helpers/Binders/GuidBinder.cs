using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RaccoonBlog.Web.Helpers.Binders
{
	/// <summary>
	/// Custom model binder for Guid types that returns Guid.Empty for invalid values
	/// </summary>
	public class GuidBinder : IModelBinder
	{
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if (bindingContext == null)
			{
				throw new ArgumentNullException(nameof(bindingContext));
			}

			var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

			if (valueProviderResult == ValueProviderResult.None)
			{
				bindingContext.Result = ModelBindingResult.Success(Guid.Empty);
				return Task.CompletedTask;
			}

			var value = valueProviderResult.FirstValue;

			if (string.IsNullOrEmpty(value))
			{
				bindingContext.Result = ModelBindingResult.Success(Guid.Empty);
				return Task.CompletedTask;
			}

			if (Guid.TryParse(value, out var guid))
			{
				bindingContext.Result = ModelBindingResult.Success(guid);
			}
			else
			{
				bindingContext.Result = ModelBindingResult.Success(Guid.Empty);
			}

			return Task.CompletedTask;
		}
	}

	/// <summary>
	/// Model binder provider for Guid types
	/// </summary>
	public class GuidBinderProvider : IModelBinderProvider
	{
		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (context.Metadata.ModelType == typeof(Guid) || context.Metadata.ModelType == typeof(Guid?))
			{
				return new GuidBinder();
			}

			return null;
		}
	}
}
