using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SwingCommon;
using SwingDataViewer.Entities;
using SwingDataViewer.Models;

namespace SwingDataViewer.Services
{
	public class TableService
	{
		private readonly CloudStorageAccount _storageAccount;
		private readonly ILogger _logger;
		public TableService(CloudStorageAccount storageAccount, ILogger<TableService> logger)
		{
			_storageAccount = storageAccount;
			_logger = logger;
		}

		public async Task<SwingLoggerEntity[]> GetRegisterdLoggers()
		{
			var tableClient = _storageAccount.CreateCloudTableClient();
			var loggerTable = tableClient.GetTableReference("SwingLogger");
			var tableQuery = new TableQuery<SwingLoggerEntity>();
			tableQuery.FilterString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0");
			var result = new List<SwingLoggerEntity>();
			TableContinuationToken token = null;
			do
			{
				var entities = await loggerTable.ExecuteQuerySegmentedAsync(tableQuery, token);
				result.AddRange(entities);
				token = entities.ContinuationToken;
			} while (token != null);
			return result.ToArray();
		}

		public async Task<string> RegisterNewId(RegisterViewModel viewModel)
		{
			var tableClient = _storageAccount.CreateCloudTableClient();
			var loggerTable = tableClient.GetTableReference("SwingLogger");
			var tableQuery = new TableQuery<SwingLoggerEntity>();
			tableQuery.FilterString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0");
			var list = new List<SwingLoggerEntity>();
			TableContinuationToken token = null;
			do
			{
				var entities = await loggerTable.ExecuteQuerySegmentedAsync(tableQuery, token);
				list.AddRange(entities);
				token = entities.ContinuationToken;
			} while (token != null);

			if (list.Any(u => u.Id == viewModel.Id)) return null;

			var salt = new byte[256 / 8];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(salt);
			}
			var passwordHash = "";
			using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(viewModel.Password, salt, 10000))
			{
				passwordHash = Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(256 / 8));
			}

			var deviceId = NumericFunctions.Scramble(list.Count + 1).ToString("d6");

			var logger = new SwingLoggerEntity
			{
				PartitionKey = "0",
				RowKey = deviceId,
				DeviceId = deviceId,
				Id = viewModel.Id,
				Password = passwordHash,
				Salt = Convert.ToBase64String(salt),
				Name = viewModel.Name,
				Public = viewModel.Public
			};

			await loggerTable.ExecuteAsync(TableOperation.InsertOrMerge(logger));
			return deviceId;
		}

		public async Task<SwingLoggerEntity> GetLogger(string id, string password)
		{
			var tableClient = _storageAccount.CreateCloudTableClient();
			var loggerTable = tableClient.GetTableReference("SwingLogger");
			var tableQuery = new TableQuery<SwingLoggerEntity>();
			tableQuery.FilterString = TableQuery.CombineFilters(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0"),
				TableOperators.And,
				TableQuery.GenerateFilterCondition("Id", QueryComparisons.Equal, id));
			var userEntities = await loggerTable.ExecuteQuerySegmentedAsync(tableQuery, null);
			var user = userEntities.FirstOrDefault();
			if (user == null) return null;


			var salt = Convert.FromBase64String(user.Salt);
			using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 10000))
			{
				if (user.Password != Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(256 / 8)))
				{
					_logger.LogInformation($"Invalid password. User: {id}");
					return null;
				}
			}

			return user;
		}

		/// <summary>
		/// こっちは認証済みユーザー用
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<SwingLoggerEntity> GetLogger(string id)
		{
			var tableClient = _storageAccount.CreateCloudTableClient();
			var loggerTable = tableClient.GetTableReference("SwingLogger");
			var tableQuery = new TableQuery<SwingLoggerEntity>();
			tableQuery.FilterString = TableQuery.CombineFilters(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0"),
				TableOperators.And,
				TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
			var userEntities = await loggerTable.ExecuteQuerySegmentedAsync(tableQuery, null);
			return userEntities.FirstOrDefault();
		}

		public async Task<bool> UpdateLogger(string id, string name, bool canPublic)
		{
			var tableClient = _storageAccount.CreateCloudTableClient();
			var loggerTable = tableClient.GetTableReference("SwingLogger");
			var tableQuery = new TableQuery<SwingLoggerEntity>();
			tableQuery.FilterString = TableQuery.CombineFilters(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0"),
				TableOperators.And,
				TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
			var userEntities = await loggerTable.ExecuteQuerySegmentedAsync(tableQuery, null);
			var user = userEntities.FirstOrDefault();
			if (user == null) return false;

			user.Name = name;
			user.Public = canPublic;
			await loggerTable.ExecuteAsync(TableOperation.Merge(user));
			return true;
		}

		public async Task<UserModel> Authenticate(LoginViewModel loginViewModel)
		{
			var logger = await GetLogger(loginViewModel.Id, loginViewModel.Password);
			return logger == null ? null : new UserModel(logger.DeviceId, logger.Name);
		}


		public async Task<SwingDataModel[]> GetSwingDatas(string id)
		{
			var tableClient = _storageAccount.CreateCloudTableClient();
			var swingTable = tableClient.GetTableReference("SwingData");

			var tableQuery = new TableQuery<SwingDataEntity>();
			tableQuery.FilterString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id);

			var result = new List<SwingDataModel>();
			TableContinuationToken token = null;
			do
			{
				var entities = await swingTable.ExecuteQuerySegmentedAsync(tableQuery, token);
				result.AddRange(entities.Select(e => new SwingDataModel
				{
					RowKey = e.RowKey,
					DateTime = $"{e.LocalTime.LocalDateTime:yyyy/MM/dd HH:mm:ss}",
					Date = e.LocalTime.LocalDateTime.ToString("yyyy/MM/dd"),
					ClubType = (ClubType)e.Club,
					Club = ((ClubType)e.Club).ToString(),
					HeadSpeed = e.HeadSpeed / 10.0,
					BallSpeed = e.BallSpeed / 10.0,
					Meet = e.Meet / 100.0,
					Distance = e.Distance
				}));
				token = entities.ContinuationToken;
			} while (token != null);
			return result.ToArray();
		}

		public async Task DeleteSwingData(string id, string row)
		{
			var tableClient = _storageAccount.CreateCloudTableClient();
			var swingTable = tableClient.GetTableReference("SwingData");
			var tableQuery = new TableQuery<SwingDataEntity>();
			tableQuery.FilterString = TableQuery.CombineFilters(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id),
				TableOperators.And,
				TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, row));
			var data = (await swingTable.ExecuteQuerySegmentedAsync(tableQuery, null)).FirstOrDefault();
			if (data != null) await swingTable.ExecuteAsync(TableOperation.Delete(data));

			var logger = await GetLogger(id);
			if (logger == null) return;
			var loggerTable = tableClient.GetTableReference("SwingLogger");
			await loggerTable.ExecuteAsync(TableOperation.Merge(logger));
		}


		public async Task<SwingSummaryEntity[]> GetSummary(DateTime targetMonth)
		{
			var tableClient = _storageAccount.CreateCloudTableClient();
			var summaryTable = tableClient.GetTableReference("SwingSummary");

			var tableQuery = new TableQuery<SwingSummaryEntity>();
			tableQuery.FilterString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, targetMonth.ToString("yyyyMM"));

			var result = new List<SwingSummaryEntity>();
			TableContinuationToken token = null;
			do
			{
				var entities = await summaryTable.ExecuteQuerySegmentedAsync(tableQuery, token);
				result.AddRange(entities);
				token = entities.ContinuationToken;
			} while (token != null);
			return result.ToArray();
		}

		public async Task<StatisticsData[]> GetStatistics(string id)
		{
			var tableClient = _storageAccount.CreateCloudTableClient();
			var table = tableClient.GetTableReference("SwingStatistics");

			var from = DateTime.UtcNow.AddMonths(-12);
			var tableQuery = new TableQuery<SwingStatisticsEntity>();
			tableQuery.FilterString = TableQuery.CombineFilters(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id),
				TableOperators.And,
				TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, $"{from:yyyyMM_ZZ}"));

			var result = new List<StatisticsData>();
			TableContinuationToken token = null;

			do
			{
				var entities = await table.ExecuteQuerySegmentedAsync(tableQuery, token);
				foreach (var e in entities)
				{
					var data = result.FirstOrDefault(r => r.Time == e.Time && r.Club == (ClubType)e.Club);
					if (data == null)
					{
						data = new StatisticsData { Time = e.Time, Club = (ClubType)e.Club };
						result.Add(data);
					}

					switch ((SwingStatisticsEntity.StatisticsType)e.Type)
					{
					case SwingStatisticsEntity.StatisticsType.HeadSpeedAverage:
						data.HeadSpeed = e.Result / 10;
						break;
					case SwingStatisticsEntity.StatisticsType.BallSpeedAverage:
						data.BallSpeed = e.Result / 10;
						break;
					case SwingStatisticsEntity.StatisticsType.DistanceAverage:
						data.Distance = e.Result;
						break;
					case SwingStatisticsEntity.StatisticsType.MeetAverage:
						data.Meet = e.Result / 100;
						break;
					}
				}

				token = entities.ContinuationToken;
			} while (token != null);
			return result.OrderBy(r => r.Club).ThenByDescending(r => r.Time).ToArray();
		}
	}
}
