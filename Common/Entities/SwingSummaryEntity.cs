using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using SwingCommon.Enum;

namespace SwingCommon.Entities
{
	public class SwingSummaryEntity : ITableEntity
	{
		public string PartitionKey { get; set; }
		public string RowKey { get; set; }
		public DateTimeOffset? Timestamp { get; set; }
		public ETag ETag { get; set; }

		// PartitionKey : yyyyMM
		// RowKey : LoggerId + SummaryType

		public string DeviceId { get; set; }
		public long Result { get; set; }
		public int Type { get; set; }   // SummaryType
		public SummaryType DataType => (SummaryType)Type;


		private static async Task<TableClient> GetTableClientAsync(TableServiceClient tableServiceClient)
		{
			var tableName = "SwingSummary";
			return tableServiceClient.GetTableClient(tableName);
		}

		public static async Task<AsyncPageable<SwingSummaryEntity>> QueryAsync(TableServiceClient tableServiceClient, Expression<Func<SwingSummaryEntity, bool>> filter)
		{
			var tableClient = await GetTableClientAsync(tableServiceClient);
			return tableClient.QueryAsync(filter);
		}

		public static async Task UpsertAsync(TableServiceClient tableServiceClient, SwingSummaryEntity entity)
		{
			var tableClient = await GetTableClientAsync(tableServiceClient);
			await tableClient.UpsertEntityAsync(entity);
		}

	}
}
