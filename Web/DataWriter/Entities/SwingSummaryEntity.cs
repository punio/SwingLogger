using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace DataWriter.Entities
{
	public class SwingSummaryEntity : TableEntity
	{
		// PartitionKey : yyyyMM
		// RowKey : LoggerId + SummaryType

		public enum SummaryType
		{
			TotalDistance,
			MaxHeadSpeed,
			MinHeadSpeed,
			MaxMeetRate,
			MinMeetRate,
			MaxDistance,
			MinDistance,
			TotalBalls
		}

		public long Result { get; set; }
		public int Type { get; set; }
	}
}
