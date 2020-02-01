using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DataWriter.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using SwingCommon;

namespace DataWriter
{
	public static class AggregateOrchestrator
	{
		[FunctionName("AggregateOrchestrator")]
		public static async Task RunOrchestrator(
			[OrchestrationTrigger] IDurableOrchestrationContext context)
		{
			var loggers = await context.CallActivityAsync<SwingLoggerEntity[]>("AggregateOrchestrator_LoggerList", null);
			foreach (var logger in loggers)
			{
				if ((DateTimeOffset.UtcNow - logger.Timestamp).TotalDays >= 3.1) continue;  // 更新無ければ集計は意味無いからね (漏れないと思うけど一応更新後3日間は集計)
				await context.CallActivityAsync("AggregateOrchestrator_Aggregate", logger.DeviceId);
			}
		}

		[FunctionName("AggregateOrchestrator_LoggerList")]
		public static async Task<SwingLoggerEntity[]> LoggerList([ActivityTrigger] IDurableActivityContext context,
			Binder binder,
			ILogger log)
		{
			var table = await binder.BindAsync<CloudTable>(new TableAttribute("SwingLogger"));
			var query = new TableQuery<SwingLoggerEntity>();
			query.FilterString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0");
			TableContinuationToken token = null;
			var result = new List<SwingLoggerEntity>();
			do
			{
				var entities = await table.ExecuteQuerySegmentedAsync(query, token);
				result.AddRange(entities);
				token = entities.ContinuationToken;
			} while (token != null);

			return result.ToArray();
		}

		[FunctionName("AggregateOrchestrator_Aggregate")]
		public static async Task Aggregate([ActivityTrigger] IDurableActivityContext activityContext,
			Binder binder,
			ExecutionContext context,
			ILogger log)
		{
			var logger = activityContext.GetInput<string>();

			var targetDayList = new List<DateTime>();
			#region 集計 (前日までの月単位)
			var toDay = DateTime.UtcNow.AddDays(-1);
			if (toDay.Day <= 1)
			{
				targetDayList.Add(toDay.AddMonths(-1));
			}
			targetDayList.Add(toDay);
			#endregion

			foreach (var targetDay in targetDayList) {
				var to = new DateTime(targetDay.Year, targetDay.Month, DateTime.DaysInMonth(targetDay.Year, targetDay.Month), 23, 59, 59, DateTimeKind.Utc);
				var from = new DateTime(to.Year, to.Month, 1, 0, 0, 0, DateTimeKind.Utc);
				var rowKeyTo = $"{DateTime.MaxValue.Ticks - from.Ticks:d19}_{Guid.Empty}";
				var rowKeyFrom = $"{DateTime.MaxValue.Ticks - (to.Ticks + 600000000):d19}_ffffffff-ffff-ffff-ffff-ffffffffffff";  // TimeSpan.FromMinutes(1).Ticks

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

				#region 集計
				var summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.TotalDistance}",
					DeviceId = logger,
					Type = (int)SwingSummaryEntity.SummaryType.TotalDistance,
					Result = temp.Sum(d => d.Distance)
				};
				tableOperations.Add(TableOperation.InsertOrMerge(summary));

				summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.MaxHeadSpeed}",
					DeviceId = logger,
					Type = (int)SwingSummaryEntity.SummaryType.MaxHeadSpeed,
					Result = temp.Max(d => d.HeadSpeed)
				};
				tableOperations.Add(TableOperation.InsertOrMerge(summary));

				summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.MinHeadSpeed}",
					DeviceId = logger,
					Type = (int)SwingSummaryEntity.SummaryType.MinHeadSpeed,
					Result = temp.Min(d => d.HeadSpeed)
				};
				tableOperations.Add(TableOperation.InsertOrMerge(summary));

				summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.MaxMeetRate}",
					DeviceId = logger,
					Type = (int)SwingSummaryEntity.SummaryType.MaxMeetRate,
					Result = temp.Max(d => d.Meet)
				};
				tableOperations.Add(TableOperation.InsertOrMerge(summary));

				summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.MinMeetRate}",
					DeviceId = logger,
					Type = (int)SwingSummaryEntity.SummaryType.MinMeetRate,
					Result = temp.Min(d => d.Meet)
				};
				tableOperations.Add(TableOperation.InsertOrMerge(summary));

				summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.TotalBalls}",
					DeviceId = logger,
					Type = (int)SwingSummaryEntity.SummaryType.TotalBalls,
					Result = temp.Length
				};
				tableOperations.Add(TableOperation.InsertOrMerge(summary));

				summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.MaxDistance}",
					DeviceId = logger,
					Type = (int)SwingSummaryEntity.SummaryType.MaxDistance,
					Result = temp.Max(d => d.Distance)
				};
				tableOperations.Add(TableOperation.InsertOrMerge(summary));

				summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SwingSummaryEntity.SummaryType.MinDistance}",
					DeviceId = logger,
					Type = (int)SwingSummaryEntity.SummaryType.MinDistance,
					Result = temp.Min(d => d.Distance)
				};
				tableOperations.Add(TableOperation.InsertOrMerge(summary));


				var summaryTable = await binder.BindAsync<CloudTable>(new TableAttribute("SwingSummary"));
				await summaryTable.CreateIfNotExistsAsync();
				await summaryTable.ExecuteBatchAsync(tableOperations);
				tableOperations.Clear();
				#endregion

				#region 統計
				// クラブ毎の平均値とか
				foreach (var club in Enum.GetValues(typeof(ClubType)))
				{
					var clubData = temp.Where(s => s.Club == (int)club).ToArray();
					if (clubData.Length <= 0) continue;

					var stat = new SwingStatisticsEntity
					{
						PartitionKey = logger,
						RowKey = $"{from:yyyyMM}_{SwingStatisticsEntity.StatisticsType.HeadSpeedAverage}_{club}",
						Time = from,
						Club = (int)club,
						Type = (int)SwingStatisticsEntity.StatisticsType.HeadSpeedAverage,
						Result = Average(clubData.Select(c => c.HeadSpeed).ToArray())
					};
					tableOperations.Add(TableOperation.InsertOrMerge(stat));

					stat = new SwingStatisticsEntity
					{
						PartitionKey = logger,
						RowKey = $"{from:yyyyMM}_{SwingStatisticsEntity.StatisticsType.BallSpeedAverage}_{club}",
						Time = from,
						Club = (int)club,
						Type = (int)SwingStatisticsEntity.StatisticsType.BallSpeedAverage,
						Result = Average(clubData.Select(c => c.BallSpeed).ToArray())
					};
					tableOperations.Add(TableOperation.InsertOrMerge(stat));

					stat = new SwingStatisticsEntity
					{
						PartitionKey = logger,
						RowKey = $"{from:yyyyMM}_{SwingStatisticsEntity.StatisticsType.DistanceAverage}_{club}",
						Time = from,
						Club = (int)club,
						Type = (int)SwingStatisticsEntity.StatisticsType.DistanceAverage,
						Result = Average(clubData.Select(c => c.Distance).ToArray())
					};
					tableOperations.Add(TableOperation.InsertOrMerge(stat));

					stat = new SwingStatisticsEntity
					{
						PartitionKey = logger,
						RowKey = $"{from:yyyyMM}_{SwingStatisticsEntity.StatisticsType.MeetAverage}_{club}",
						Time = from,
						Club = (int)club,
						Type = (int)SwingStatisticsEntity.StatisticsType.MeetAverage,
						Result = Average(clubData.Select(c => c.Meet).ToArray())
					};
					tableOperations.Add(TableOperation.InsertOrMerge(stat));

					stat = new SwingStatisticsEntity
					{
						PartitionKey = logger,
						RowKey = $"{from:yyyyMM}_{SwingStatisticsEntity.StatisticsType.TotalBalls}_{club}",
						Time = from,
						Club = (int)club,
						Type = (int)SwingStatisticsEntity.StatisticsType.TotalBalls,
						Result = clubData.Count()
					};
					tableOperations.Add(TableOperation.InsertOrMerge(stat));
				}
				var statTable = await binder.BindAsync<CloudTable>(new TableAttribute("SwingStatistics"));
				await statTable.CreateIfNotExistsAsync();
				await statTable.ExecuteBatchAsync(tableOperations);
				tableOperations.Clear();
				#endregion
			}
		}

		private static double Average(int[] values)
		{
			if (values.Length < 20) return values.Average();    // 要素数が少ない時は全部の平均
			var exCount = (int)(values.Length * 0.05);  // 前後5%を除外
			return values.OrderBy(v => v).Skip(exCount).Take(values.Length - exCount * 2).Average();
		}

	}
}