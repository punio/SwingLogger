using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SwingCommon;

namespace SwingDataViewer.Models
{
	public class SwingDataModel
	{
		[JsonIgnore]
		public string RowKey { get; set; }
		[JsonIgnore]
		public string DateTime { get; set; }

		public string Date { get; set; }

		public ClubType ClubType { get; set; }
		public string Club { get; set; }
		public double HeadSpeed { get; set; }
		public double BallSpeed { get; set; }
		public int Distance { get; set; }
		public double Meet { get; set; }

		public string Tag { get; set; }
	}
}
