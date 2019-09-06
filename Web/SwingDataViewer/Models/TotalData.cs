using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwingDataViewer.Models
{
	public class TotalData
	{
		public Date[] Date { get; set; }
		public SwingDataModel[] Swing { get; set; }
	}

	public class Date
	{
		public string Month { get; set; }
		public string Day { get; set; }

		public Date(string date)
		{
			if (date.Length > 7) Month = date.Substring(0, 7);
			Day = date;
		}
	}
}
