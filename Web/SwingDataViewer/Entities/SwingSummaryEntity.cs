using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using SwingCommon.Enum;

namespace SwingDataViewer.Entities
{
	public class SwingSummaryEntity : TableEntity
	{
		// PartitionKey : yyyyMM
		// RowKey : LoggerId + SummaryType

		public string DeviceId { get; set; }
		public long Result { get; set; }
		public int Type { get; set; }   // SummaryType
		public SummaryType DataType => (SummaryType)Type;
	}
}
