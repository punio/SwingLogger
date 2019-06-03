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

		public async Task<string> RegisterNewId(string id, string password)
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

			if (list.Any(u => u.Id == id)) return null;

			var salt = new byte[256 / 8];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(salt);
			}
			var passwordHash = "";
			using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 10000))
			{
				passwordHash = Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(256 / 8));
			}

			var deviceId = NumericFunctions.Scramble(list.Count + 1).ToString("d6");

			var logger = new SwingLoggerEntity
			{
				PartitionKey = "0",
				RowKey = deviceId,
				DeviceId = deviceId,
				Id = id,
				Password = passwordHash,
				Salt = Convert.ToBase64String(salt)
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

		public async Task<SwingDataModel[]> GetSwingDatasAsync(string id)
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
					Date = e.Time.ToString("yyyy/MM/dd"),
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
	}
}
