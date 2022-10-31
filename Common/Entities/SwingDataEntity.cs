using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;

namespace SwingCommon.Entities
{
	public class SwingDataEntity : ITableEntity
	{
		public string PartitionKey { get; set; }
		public string RowKey { get; set; }
		public DateTimeOffset? Timestamp { get; set; }
		public ETag ETag { get; set; }

		public string User { get; set; }

		public DateTimeOffset Time { get; set; }
		public string TimeOffset { get; set; }    // DateTimeOffset -> JSONで時差が消えてしまうので・・
		public string Dump { get; set; }

		public int Club { get; set; }
		public int HeadSpeed { get; set; }
		public int BallSpeed { get; set; }
		public int Distance { get; set; }
		public int Meet { get; set; }

		public string Tag { get; set; }



		private DateTimeOffset _localTime = DateTimeOffset.MinValue;
		[JsonIgnore]
		public DateTimeOffset LocalTime
		{
			get
			{
				if (_localTime > DateTimeOffset.MinValue) return _localTime;
				_localTime = TimeSpan.TryParse(TimeOffset, out var offset) ? Time.DateTime.Add(offset) : Time;
				return _localTime;
			}
		}



		private static async Task<TableClient> GetTableClientAsync(TableServiceClient tableServiceClient)
		{
			var tableName = "SwingData";
			return tableServiceClient.GetTableClient(tableName);
		}

		public static async Task<AsyncPageable<SwingDataEntity>> QueryAsync(TableServiceClient tableServiceClient, Expression<Func<SwingDataEntity, bool>> filter)
		{
			var tableClient = await GetTableClientAsync(tableServiceClient);
			return tableClient.QueryAsync(filter);
		}

		public static async Task UpsertAsync(TableServiceClient tableServiceClient, SwingDataEntity entity)
		{
			var tableClient = await GetTableClientAsync(tableServiceClient);
			await tableClient.UpsertEntityAsync(entity);
		}

		public static async Task DeleteAsync(TableServiceClient tableServiceClient, SwingDataEntity entity)
		{
			var tableClient = await GetTableClientAsync(tableServiceClient);
			await tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey);
		}
	}
}
