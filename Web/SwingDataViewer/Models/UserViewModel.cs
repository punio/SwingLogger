using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwingDataViewer.Models
{
	public class UserViewModel
	{
		[DisplayName("NAME")]
		public string Name { get; set; }

		[DisplayName("公開")]
		public bool Public { get; set; }


		public bool HaveResult { get; set; }
		public bool Error { get; set; }
		public string Message { get; set; }
	}
}
