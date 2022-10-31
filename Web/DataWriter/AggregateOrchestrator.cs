using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using SwingCommon;
using SwingCommon.Entities;
using SwingCommon.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataWriter
{
	public class AggregateOrchestrator
	{
		private readonly ILogger _logger;
		private readonly TableServiceClient _tableServiceClient;

		public AggregateOrchestrator(TableServiceClient tableServiceClient, ILogger<AggregateOrchestrator> logger)
		{
			_logger = logger;
			_tableServiceClient = tableServiceClient;
		}

		[FunctionName("AggregateOrchestrator")]
		public async Task RunOrchestrator(
			[OrchestrationTrigger] IDurableOrchestrationContext context)
		{
			var loggers = await context.CallActivityAsync<SwingLoggerEntity[]>("AggregateOrchestrator_LoggerList", null);
			foreach (var logger in loggers)
			{
				if ((DateTimeOffset.UtcNow - logger.Timestamp.Value).TotalDays >= 3.1) continue;  // 更新無ければ集計は意味無いからね (漏れないと思うけど一応更新後3日間は集計)
				await context.CallActivityAsync("AggregateOrchestrator_Aggregate", logger.DeviceId);
			}
		}

		[FunctionName("AggregateOrchestrator_LoggerList")]
		public async Task<SwingLoggerEntity[]> LoggerList([ActivityTrigger] IDurableActivityContext context,
			ILogger log)
		{
			var result = new List<SwingLoggerEntity>();
			var entities = await SwingLoggerEntity.QueryAsync(_tableServiceClient, x => x.PartitionKey == "0");
			await foreach (var e in entities) result.Add(e);

			return result.ToArray();
		}

		[FunctionName("AggregateOrchestrator_Aggregate")]
		public async Task Aggregate([ActivityTrigger] IDurableActivityContext activityContext,
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

			foreach (var targetDay in targetDayList)
			{
				var to = new DateTime(targetDay.Year, targetDay.Month, DateTime.DaysInMonth(targetDay.Year, targetDay.Month), 23, 59, 59, DateTimeKind.Utc);
				var from = new DateTime(to.Year, to.Month, 1, 0, 0, 0, DateTimeKind.Utc);
				var rowKeyTo = $"{DateTime.MaxValue.Ticks - from.Ticks:d19}_{Guid.Empty}";
				var rowKeyFrom = $"{DateTime.MaxValue.Ticks - (to.Ticks + 600000000):d19}_ffffffff-ffff-ffff-ffff-ffffffffffff";  // TimeSpan.FromMinutes(1).Ticks

				log.LogInformation($"Correct : {from} - {to}");

				var dataEntities = new List<SwingDataEntity>();
				var entities = await SwingDataEntity.QueryAsync(_tableServiceClient, x => x.PartitionKey == logger
								  && string.Compare(x.RowKey, rowKeyFrom, StringComparison.Ordinal) >= 0 && string.Compare(x.RowKey, rowKeyTo, StringComparison.Ordinal) <= 0);
				await foreach (var e in entities)
				{
					if (e.Club != (int)ClubType.PT && e.BallSpeed > 0) dataEntities.Add(e);
				}

				var temp = dataEntities.ToArray();
				if (temp.Length <= 0) return;

				#region 集計
				var summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SummaryType.TotalDistance}",
					DeviceId = logger,
					Type = (int)SummaryType.TotalDistance,
					Result = temp.Sum(d => d.Distance)
				};
				await SwingSummaryEntity.UpsertAsync(_tableServiceClient, summary);

				var hsList = ExcludeErrors(temp).ToArray();
				if (hsList.Length > 0)
				{
					summary = new SwingSummaryEntity
					{
						PartitionKey = $"{from:yyyyMM}",
						RowKey = $"{logger}_{SummaryType.MaxHeadSpeed}",
						DeviceId = logger,
						Type = (int)SummaryType.MaxHeadSpeed,
						Result = hsList.Max(d => d.HeadSpeed)
					};
					await SwingSummaryEntity.UpsertAsync(_tableServiceClient, summary);
				}

				summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SummaryType.MinHeadSpeed}",
					DeviceId = logger,
					Type = (int)SummaryType.MinHeadSpeed,
					Result = temp.Min(d => d.HeadSpeed)
				};
				await SwingSummaryEntity.UpsertAsync(_tableServiceClient, summary);

				summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SummaryType.MaxMeetRate}",
					DeviceId = logger,
					Type = (int)SummaryType.MaxMeetRate,
					Result = temp.Max(d => d.Meet)
				};
				await SwingSummaryEntity.UpsertAsync(_tableServiceClient, summary);

				summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SummaryType.MinMeetRate}",
					DeviceId = logger,
					Type = (int)SummaryType.MinMeetRate,
					Result = temp.Min(d => d.Meet)
				};
				await SwingSummaryEntity.UpsertAsync(_tableServiceClient, summary);

				summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SummaryType.TotalBalls}",
					DeviceId = logger,
					Type = (int)SummaryType.TotalBalls,
					Result = temp.Length
				};
				await SwingSummaryEntity.UpsertAsync(_tableServiceClient, summary);

				summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SummaryType.MaxDistance}",
					DeviceId = logger,
					Type = (int)SummaryType.MaxDistance,
					Result = temp.Max(d => d.Distance)
				};
				await SwingSummaryEntity.UpsertAsync(_tableServiceClient, summary);

				summary = new SwingSummaryEntity
				{
					PartitionKey = $"{from:yyyyMM}",
					RowKey = $"{logger}_{SummaryType.MinDistance}",
					DeviceId = logger,
					Type = (int)SummaryType.MinDistance,
					Result = temp.Min(d => d.Distance)
				};
				await SwingSummaryEntity.UpsertAsync(_tableServiceClient, summary);

				var count = temp.Count(d => d.Distance == 50);
				if (count > 0)
				{
					summary = new SwingSummaryEntity
					{
						PartitionKey = $"{from:yyyyMM}",
						RowKey = $"{logger}_{SummaryType.Just50Yard}",
						DeviceId = logger,
						Type = (int)SummaryType.Just50Yard,
						Result = count
					};
					await SwingSummaryEntity.UpsertAsync(_tableServiceClient, summary);
				}
				count = temp.Count(d => d.Distance == 100);
				if (count > 0)
				{
					summary = new SwingSummaryEntity
					{
						PartitionKey = $"{from:yyyyMM}",
						RowKey = $"{logger}_{SummaryType.Just100Yard}",
						DeviceId = logger,
						Type = (int)SummaryType.Just100Yard,
						Result = count
					};
					await SwingSummaryEntity.UpsertAsync(_tableServiceClient, summary);
				}
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
					await SwingStatisticsEntity.UpsertAsync(_tableServiceClient, stat);

					stat = new SwingStatisticsEntity
					{
						PartitionKey = logger,
						RowKey = $"{from:yyyyMM}_{SwingStatisticsEntity.StatisticsType.BallSpeedAverage}_{club}",
						Time = from,
						Club = (int)club,
						Type = (int)SwingStatisticsEntity.StatisticsType.BallSpeedAverage,
						Result = Average(clubData.Select(c => c.BallSpeed).ToArray())
					};
					await SwingStatisticsEntity.UpsertAsync(_tableServiceClient, stat);

					stat = new SwingStatisticsEntity
					{
						PartitionKey = logger,
						RowKey = $"{from:yyyyMM}_{SwingStatisticsEntity.StatisticsType.DistanceAverage}_{club}",
						Time = from,
						Club = (int)club,
						Type = (int)SwingStatisticsEntity.StatisticsType.DistanceAverage,
						Result = Average(clubData.Select(c => c.Distance).ToArray())
					};
					await SwingStatisticsEntity.UpsertAsync(_tableServiceClient, stat);

					stat = new SwingStatisticsEntity
					{
						PartitionKey = logger,
						RowKey = $"{from:yyyyMM}_{SwingStatisticsEntity.StatisticsType.MeetAverage}_{club}",
						Time = from,
						Club = (int)club,
						Type = (int)SwingStatisticsEntity.StatisticsType.MeetAverage,
						Result = Average(clubData.Select(c => c.Meet).ToArray())
					};
					await SwingStatisticsEntity.UpsertAsync(_tableServiceClient, stat);

					stat = new SwingStatisticsEntity
					{
						PartitionKey = logger,
						RowKey = $"{from:yyyyMM}_{SwingStatisticsEntity.StatisticsType.TotalBalls}_{club}",
						Time = from,
						Club = (int)club,
						Type = (int)SwingStatisticsEntity.StatisticsType.TotalBalls,
						Result = clubData.Count()
					};
					await SwingStatisticsEntity.UpsertAsync(_tableServiceClient, stat);
				}
				#endregion
			}
		}

		private static double Average(int[] values)
		{
			if (values.Length < 20) return values.Average();    // 要素数が少ない時は全部の平均
			var exCount = (int)(values.Length * 0.05);  // 前後5%を除外
			return values.OrderBy(v => v).Skip(exCount).Take(values.Length - exCount * 2).Average();
		}

		/// <summary>
		/// ウッドでミート率1.10未満は測定エラーとして除外(最大ヘッドスピードの計算用)
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private static IEnumerable<SwingDataEntity> ExcludeErrors(SwingDataEntity[] data)
		{
			return data.Where(d => d.Club > (int)ClubType.I6 ||
								   ((int)ClubType.W1 <= d.Club && d.Club <= (int)ClubType.W9 && d.Meet > 110) ||
								   ((int)ClubType.U2 <= d.Club && d.Club <= (int)ClubType.I6 && d.Meet > 105));
		}
	}
}