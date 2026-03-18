using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RaccoonBlog.Web.Helpers.Results
{
	public class JsonNetResult : IActionResult
	{
		public JsonNetResult()
		{
		}

		public JsonNetResult(object responseBody)
		{
			ResponseBody = responseBody;
		}

		public JsonNetResult(object responseBody, JsonSerializerSettings settings)
			: this(responseBody)
		{
			Settings = settings;
		}

		/// <summary>Gets or sets the serialiser settings</summary> 
		public JsonSerializerSettings Settings { get; set; }

		/// <summary>Gets or sets the encoding of the response</summary> 
		public Encoding ContentEncoding { get; set; }

		/// <summary>Gets or sets the content type for the response</summary> 
		public string ContentType { get; set; }

		/// <summary>Gets or sets the body of the response</summary> 
		public object ResponseBody { get; set; }

		/// <summary>Gets the formatting types depending on whether we are in debug mode</summary> 
		private Formatting Formatting
		{
			get { return Debugger.IsAttached ? Formatting.Indented : Formatting.None; }
		}

		/// <summary> 
		/// Serialises the response and writes it out to the response object 
		/// </summary> 
		/// <param name="context">The execution context</param> 
		public async Task ExecuteResultAsync(ActionContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			HttpResponse response = context.HttpContext.Response;

			// set content type 
			if (!string.IsNullOrEmpty(ContentType))
			{
				response.ContentType = ContentType;
			}
			else
			{
				response.ContentType = "application/json";
			}

			if (ResponseBody != null)
			{
				var json = JsonConvert.SerializeObject(ResponseBody, Formatting, Settings);

                await response.WriteAsync(json);
            }
		}
	}
}