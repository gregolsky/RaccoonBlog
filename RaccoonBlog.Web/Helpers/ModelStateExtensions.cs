using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RaccoonBlog.Web.Helpers
{
	public static class ModelStateExtensions
	{
		/// <summary>
		/// Gets the first error message from ModelState
		/// </summary>
		public static string FirstErrorMessage(this ModelStateDictionary modelState)
		{
			if (modelState == null || modelState.IsValid)
			{
				return null;
			}

			var state = modelState.Values.FirstOrDefault(v => v.Errors.Count > 0);

			if (state == null) 
				return null;

			var message = state.Errors
				.Where(error => !string.IsNullOrEmpty(error.ErrorMessage))
				.Select(error => error.ErrorMessage)
				.FirstOrDefault();
			
			return message;
		}
	}
}