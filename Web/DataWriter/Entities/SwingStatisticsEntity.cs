using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace DataWriter.Entities
{
	public class SwingStatisticsEntity : TableEntity
	{
		// PartitionKey : LoggerId
		// RowKey : yyyyMM + StatisticsType + Club

		public enum StatisticsType
		{
			HeadSpeedAverage, BallSpeedAverage, DistanceAverage, MeetAverage
		}

		public DateTime Time { get; set; }

		public int Club { get; set; }
		public double Result { get; set; }

		public int Type { get; set; }   // StatisticsType
	}
}
