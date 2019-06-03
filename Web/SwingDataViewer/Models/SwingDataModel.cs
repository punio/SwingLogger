using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwingCommon;

namespace SwingDataViewer.Models
{
	public class SwingDataModel
	{
		public string Date { get; set; }

		public ClubType ClubType { get; set; }
		public string Club { get; set; }
		public double HeadSpeed { get; set; }
		public double BallSpeed { get; set; }
		public int Distance { get; set; }
		public double Meet { get; set; }
	}
}
