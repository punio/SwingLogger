using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SwingCommon;
using SwingCommon.Entities;
using SwingCommon.Function;
using SwingDataViewer.Models;

namespace SwingDataViewer.Services
{
	public class TableService
	{
		private readonly TableServiceClient _tableServiceClient;
		private readonly ILogger _logger;
		public TableService(TableServiceClient tableServiceClient, ILogger<TableService> logger)
		{
			_tableServiceClient = tableServiceClient;
			_logger = logger;
		}

		public async Task<SwingLoggerEntity[]> GetRegisterdLoggers()
		{
			var result = new List<SwingLoggerEntity>();
			var entities = await SwingLoggerEntity.QueryAsync(_tableServiceClient, x => x.PartitionKey == "0");
			await foreach (var e in entities) result.Add(e);

			return result.ToArray();
		}

		public async Task<string> RegisterNewId(RegisterViewModel viewModel)
		{
			var list = await GetRegisterdLoggers();
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

			var deviceId = NumericFunctions.Scramble(list.Length + 1).ToString("d6");

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

			await SwingLoggerEntity.UpsertAsync(_tableServiceClient, logger);
			return deviceId;
		}

		public async Task<SwingLoggerEntity> GetLogger(string id, string password)
		{
			var user = await (await SwingLoggerEntity.QueryAsync(_tableServiceClient, x => x.PartitionKey == "0" && x.Id == id)).FirstOrDefault();
			if (user == null) return null;


			var salt = Convert.FromBase64String(user.Salt);
			using var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 10000);
			if (user.Password != Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(256 / 8)))
			{
				_logger.LogInformation($"Invalid password. User: {id}");
				return null;
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
			return await (await SwingLoggerEntity.QueryAsync(_tableServiceClient, x => x.PartitionKey == "0" && x.RowKey == id)).FirstOrDefault();
		}

		public async Task<bool> UpdateLogger(string id, string name, bool canPublic)
		{
			var user = await GetLogger(id);

			user.Name = name;
			user.Public = canPublic;
			await SwingLoggerEntity.UpsertAsync(_tableServiceClient, user);
			return true;
		}

		public async Task<UserModel> Authenticate(LoginViewModel loginViewModel)
		{
			var logger = await GetLogger(loginViewModel.Id, loginViewModel.Password);
			return logger == null ? null : new UserModel(logger.DeviceId, logger.Name);
		}


		public async Task<SwingDataModel[]> GetSwingDatas(string id, DateTime from, DateTime to)
		{
			var rowKeyTo = $"{DateTime.MaxValue.Ticks - from.Ticks:d19}_{Guid.Empty}";
			to = to.AddDays(1).AddSeconds(-1);
			var rowKeyFrom = $"{DateTime.MaxValue.Ticks - to.Ticks:d19}_ffffffff-ffff-ffff-ffff-ffffffffffff";  // TimeSpan.FromMinutes(1).Ticks
			var entities = await SwingDataEntity.QueryAsync(_tableServiceClient, x => x.PartitionKey == id
					&& string.Compare(x.RowKey, rowKeyFrom, StringComparison.Ordinal) >= 0 && string.Compare(x.RowKey, rowKeyTo, StringComparison.Ordinal) <= 0);

			var result = new List<SwingDataModel>();
			await foreach (var e in entities)
			{
				result.Add(new SwingDataModel
				{
					RowKey = e.RowKey,
					DateTime = $"{e.LocalTime.LocalDateTime:yyyy/MM/dd HH:mm:ss}",
					Date = e.LocalTime.LocalDateTime.ToString("yyyy/MM/dd"),
					ClubType = (ClubType)e.Club,
					Club = ((ClubType)e.Club).ToString(),
					HeadSpeed = e.HeadSpeed / 10.0,
					BallSpeed = e.BallSpeed / 10.0,
					Meet = e.Meet / 100.0,
					Distance = e.Distance,
					Tag = e.Tag
				});
			}

			return result.ToArray();
		}

		public async Task DeleteSwingData(string id, string row)
		{
			var data = await (await SwingDataEntity.QueryAsync(_tableServiceClient, x => x.PartitionKey == id && x.RowKey == row)).FirstOrDefault();
			if (data != null) await SwingDataEntity.DeleteAsync(_tableServiceClient, data);

			var logger = await GetLogger(id);
			if (logger == null) return;
			logger.ETag = Azure.ETag.All;
			await SwingLoggerEntity.UpsertAsync(_tableServiceClient, logger);
		}


		public async Task<SwingSummaryEntity[]> GetSummary(DateTime targetMonth)
		{
			var result = new List<SwingSummaryEntity>();
			var entities = await SwingSummaryEntity.QueryAsync(_tableServiceClient, x => x.PartitionKey == targetMonth.ToString("yyyyMM"));
			await foreach (var e in entities) result.Add(e);

			return result.ToArray();
		}

		public async Task<StatisticsData[]> GetStatistics(string id)
		{
			var from = DateTime.UtcNow.AddMonths(-12);
			var result = new List<StatisticsData>();

			var entities = await SwingStatisticsEntity.QueryAsync(_tableServiceClient, x => x.PartitionKey == id && x.RowKey.CompareTo($"{from:yyyyMM_ZZ}") >= 0);
			await foreach (var e in entities)
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
				case SwingStatisticsEntity.StatisticsType.TotalBalls:
					data.Total = (int)e.Result;
					break;
				}
			}

			return result.OrderBy(r => r.Club).ThenByDescending(r => r.Time).ToArray();
		}
	}
}
