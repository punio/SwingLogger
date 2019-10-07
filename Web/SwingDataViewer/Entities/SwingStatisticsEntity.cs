using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace SwingDataViewer.Entities
{
	public class SwingStatisticsEntity : TableEntity
	{
		// PartitionKey : LoggerId
		// RowKey : yyyyMM + StatisticsType + Club

		public enum StatisticsType
		{
			HeadSpeedAverage, BallSpeedAverage, DistanceAverage, MeetAverage, TotalBalls
		}

		public DateTime Time { get; set; }

		public int Club { get; set; }
		public double Result { get; set; }

		public int Type { get; set; }   // StatisticsType
	}
}
