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
	public class SwingLoggerEntity : ITableEntity
	{
		public string PartitionKey { get; set; }
		public string RowKey { get; set; }
		public DateTimeOffset? Timestamp { get; set; }
		public ETag ETag { get; set; }

		// PartitionKey : 0

		public string DeviceId { get; set; }

		public string Id { get; set; }
		public string Salt { get; set; }
		public string Password { get; set; }
		public bool IncomingData { get; set; }

		public string Name { get; set; } = "匿名";
		public bool Public { get; set; } = true;


		private static async Task<TableClient> GetTableClientAsync(TableServiceClient tableServiceClient)
		{
			var tableName = "SwingLogger";
			return tableServiceClient.GetTableClient(tableName);
		}

		public static async Task<AsyncPageable<SwingLoggerEntity>> QueryAsync(TableServiceClient tableServiceClient, Expression<Func<SwingLoggerEntity, bool>> filter)
		{
			var tableClient = await GetTableClientAsync(tableServiceClient);
			return tableClient.QueryAsync(filter);
		}

		public static async Task UpsertAsync(TableServiceClient tableServiceClient, SwingLoggerEntity entity)
		{
			var tableClient = await GetTableClientAsync(tableServiceClient);
			await tableClient.UpsertEntityAsync(entity);
		}

	}
}
