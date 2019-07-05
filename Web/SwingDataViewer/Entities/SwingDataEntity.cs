using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace SwingDataViewer.Entities
{
	public class SwingDataEntity : TableEntity
	{
		public string User { get; set; }

		public string LocalDate { get; set; }
		public string LocalTime { get; set; }

		public DateTimeOffset Time { get; set; }
		public string Dump { get; set; }

		public int Club { get; set; }
		public int HeadSpeed { get; set; }
		public int BallSpeed { get; set; }
		public int Distance { get; set; }
		public int Meet { get; set; }
	}
}
