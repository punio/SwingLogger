using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SwingDataViewer.Models
{
	public class LoginViewModel
	{
		[Required]
		[DisplayName("ID")]
		public string Id { get; set; }

		[Required]
		[DisplayName("Password")]
		public string Password { get; set; }

		public string ReturnUrl { get; set; }
	}
}
