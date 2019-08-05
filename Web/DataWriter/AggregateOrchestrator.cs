using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DataWriter.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace DataWriter
{
	public static class AggregateOrchestrator
	{
		[FunctionName("AggregateOrchestrator")]
		public static async Task RunOrchestrator(
			[OrchestrationTrigger] DurableOrchestrationContext context)
		{
			var loggers = await context.CallActivityAsync<string[]>("AggregateOrchestrator_LoggerList", null);
			foreach (var logger in loggers)
			{
				await context.CallActivityAsync("AggregateOrchestrator_Aggregate", logger);
			}
		}

		[FunctionName("AggregateOrchestrator_LoggerList")]
		public static async Task<string[]> LoggerList([ActivityTrigger] DurableActivityContext context,
			Binder binder,
			ILogger log)
		{
			var table = await binder.BindAsync<CloudTable>(new TableAttribute("SwingLogger"));
			var query = new TableQuery<SwingLoggerEntity>();
			query.FilterString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0");
			TableContinuationToken token = null;
			var result = new List<string>();
			do
			{
				var entities = await table.ExecuteQuerySegmentedAsync(query, token);
				result.AddRange(entities.Select(e => e.DeviceId));
				token = entities.ContinuationToken;
			} while (token != null);

			return result.ToArray();
		}

		[FunctionName("AggregateOrchestrator_Aggregate")]
		public static async Task Aggregate([ActivityTrigger] DurableActivityContext activityContext,
			Binder binder,
			ExecutionContext context,
			ILogger log)
		{
			var logger = activityContext.GetInput<string>();

			#region 集計 (前日までの月単位)
			var to = DateTime.UtcNow.AddDays(-1);
			var day = to.Day;
			if (to.Day <= 1)
			{
				to = to.AddDays(-2);
				day = DateTime.DaysInMonth(to.Year, to.Month);
			}
			to = new DateTime(to.Year, to.Month, day, 23, 59, 59);
			var from = new DateTime(to.Year, to.Month, 1, 0, 0, 0);
			var rowKeyTo = $"{DateTime.MaxValue.Ticks - from.Ticks:d19}_{Guid.Empty}";
			var rowKeyFrom = $"{DateTime.MaxValue.Ticks - (to.Ticks + 600000000):d19}_ffffffff-ffff-ffff-ffff-ffffffffffff";  // TimeSpan.FromMinutes(1).Ticks
			#endregion

			log.LogInformation($"Correct : {from} - {to}");

			var table = await binder.BindAsync<CloudTable>(new TableAttribute("SwingData"));
			var query = new TableQuery<SwingDataEntity>();
			query.FilterString = TableQuery.CombineFilters(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, logger),
				TableOperators.And,
				TableQuery.CombineFilters(
					TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, rowKeyFrom),
					TableOperators.And,
					TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, rowKeyTo)));

			TableContinuationToken token = null;
			var dataEntities = new List<SwingDataEntity>();
			do
			{
				var entities = await table.ExecuteQuerySegmentedAsync(query, token);
				dataEntities.AddRange(entities.Where(e => e.BallSpeed > 0));
				token = entities.ContinuationToken;
			} while (token != null);

			var temp = dataEntities.ToArray();
			if (temp.Length <= 0) return;

			var tableOperations = new TableBatchOperation();
			var summary = new SwingSummaryEntity
			{
				PartitionKey = $"{from:yyyyMM}",
				RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.TotalDistance}",
				Type = (int)SwingSummaryEntity.SummaryType.TotalDistance,
				Result = temp.Sum(d => d.Distance)
			};
			tableOperations.Add(TableOperation.InsertOrMerge(summary));

			summary = new SwingSummaryEntity
			{
				PartitionKey = $"{from:yyyyMM}",
				RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.MaxHeadSpeed}",
				Type = (int)SwingSummaryEntity.SummaryType.MaxHeadSpeed,
				Result = temp.Max(d => d.HeadSpeed)
			};
			tableOperations.Add(TableOperation.InsertOrMerge(summary));

			summary = new SwingSummaryEntity
			{
				PartitionKey = $"{from:yyyyMM}",
				RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.MinHeadSpeed}",
				Type = (int)SwingSummaryEntity.SummaryType.MinHeadSpeed,
				Result = temp.Min(d => d.HeadSpeed)
			};
			tableOperations.Add(TableOperation.InsertOrMerge(summary));

			summary = new SwingSummaryEntity
			{
				PartitionKey = $"{from:yyyyMM}",
				RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.MaxMeetRate}",
				Type = (int)SwingSummaryEntity.SummaryType.MaxMeetRate,
				Result = temp.Max(d => d.Meet)
			};
			tableOperations.Add(TableOperation.InsertOrMerge(summary));

			summary = new SwingSummaryEntity
			{
				PartitionKey = $"{from:yyyyMM}",
				RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.MinMeetRate}",
				Type = (int)SwingSummaryEntity.SummaryType.MinMeetRate,
				Result = temp.Min(d => d.Meet)
			};
			tableOperations.Add(TableOperation.InsertOrMerge(summary));

			summary = new SwingSummaryEntity
			{
				PartitionKey = $"{from:yyyyMM}",
				RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.TotalBalls}",
				Type = (int)SwingSummaryEntity.SummaryType.TotalBalls,
				Result = temp.Length
			};
			tableOperations.Add(TableOperation.InsertOrMerge(summary));

			summary = new SwingSummaryEntity
			{
				PartitionKey = $"{from:yyyyMM}",
				RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.MaxDistance}",
				Type = (int)SwingSummaryEntity.SummaryType.MaxDistance,
				Result = temp.Max(d => d.Distance)
			};
			tableOperations.Add(TableOperation.InsertOrMerge(summary));

			summary = new SwingSummaryEntity
			{
				PartitionKey = $"{from:yyyyMM}",
				RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.MinDistance}",
				Type = (int)SwingSummaryEntity.SummaryType.MinDistance,
				Result = temp.Min(d => d.Distance)
			};
			tableOperations.Add(TableOperation.InsertOrMerge(summary));


			var summaryTable = await binder.BindAsync<CloudTable>(new TableAttribute("SwingSummary"));
			await summaryTable.CreateIfNotExistsAsync();

			await summaryTable.ExecuteBatchAsync(tableOperations);
		}

	}
}