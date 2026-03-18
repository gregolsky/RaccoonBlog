using System;
using System.ComponentModel.DataAnnotations;

namespace RaccoonBlog.Web.ViewModels
{
	[Serializable]
	public class CommentInput
	{
		[Required(ErrorMessage = "Name is required")]
		[Display(Name = "Name")]
		[StringLength(256, ErrorMessage = "The Name must not exceed 256 chars")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Email is required")]
		[Display(Name = "Email")]
		[EmailAddress(ErrorMessage = "Email is invalid")]
		public string Email { get; set; }

		[Display(Name = "Url")]
		[Url]
		public string Url { get; set; }

		[Required(ErrorMessage = "Comment is required")]
		[Display(Name = "Comments")]
		[DataType(DataType.MultilineText)]
		public string Body { get; set; }

		public bool IsSpam { get; set; }

		public string CommenterKey { get; set; }
	}
}

