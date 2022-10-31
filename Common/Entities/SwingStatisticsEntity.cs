using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;

namespace SwingCommon.Entities
{
	public class SwingStatisticsEntity : ITableEntity
	{
		public string PartitionKey { get; set; }
		public string RowKey { get; set; }
		public DateTimeOffset? Timestamp { get; set; }
		public ETag ETag { get; set; }

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


		private static async Task<TableClient> GetTableClientAsync(TableServiceClient tableServiceClient)
		{
			var tableName = "SwingStatistics";
			return tableServiceClient.GetTableClient(tableName);
		}

		public static async Task<AsyncPageable<SwingStatisticsEntity>> QueryAsync(TableServiceClient tableServiceClient, Expression<Func<SwingStatisticsEntity, bool>> filter)
		{
			var tableClient = await GetTableClientAsync(tableServiceClient);
			return tableClient.QueryAsync(filter);
		}

		public static async Task UpsertAsync(TableServiceClient tableServiceClient, SwingStatisticsEntity entity)
		{
			var tableClient = await GetTableClientAsync(tableServiceClient);
			await tableClient.UpsertEntityAsync(entity);
		}

	}
}
