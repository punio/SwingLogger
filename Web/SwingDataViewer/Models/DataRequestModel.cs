using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwingDataViewer.Models
{
	public class DataRequestModel
	{
		public string Id { get; set; }
		public string From { get; set; }
		public string To { get; set; }
		public int Offset { get; set; }
	}
}
