using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwingCommon;

namespace SwingDataViewer.Models
{
	public class StatisticsModel
	{
		public StatisticsData[] Data { get; set; }
	}

	public class StatisticsData
	{
		public DateTime Time { get; set; }

		public ClubType Club { get; set; }

		public double HeadSpeed { get; set; }
		public double BallSpeed { get; set; }
		public double Distance { get; set; }
		public double Meet { get; set; }
	}
}
