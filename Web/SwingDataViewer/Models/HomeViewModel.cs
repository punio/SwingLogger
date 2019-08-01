using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwingCommon;

namespace SwingDataViewer.Models
{
	public class HomeViewModel
	{
		public User[] Loggers { get; set; }
	}

	public class User
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}
}
