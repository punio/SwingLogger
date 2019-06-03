using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SwingDataViewer.Models
{
	public class RegisterViewModel
	{
		[Required]
		[DisplayName("ID")]
		public string Id { get; set; }

		[Required]
		[DisplayName("Password")]
		public string Password { get; set; }

		public bool HaveResult { get; set; }
		public bool Error { get; set; }
		public string Message { get; set; }
	}
}
