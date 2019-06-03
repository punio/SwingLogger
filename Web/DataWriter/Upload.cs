using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SwingCommon;

namespace DataWriter
{
	public static class Upload
	{
		[FunctionName("Upload")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
			[Table("SwingData")] CloudTable dataTable,
			[Table("SwingLogger")] CloudTable loggerTable,
			ILogger log)
		{
			SwingData request = null;
			if (req.Headers.TryGetValue("Content-Encoding", out var value))
			{
				if (value.Any(c => c == "gzip"))
				{
					using (var decompressionStream = new GZipStream(req.Body, CompressionMode.Decompress))
					{
						using (var reader = new StreamReader(decompressionStream))
						{
							request = JsonConvert.DeserializeObject<SwingData>(reader.ReadToEnd());
						}
					}
				}
			}
			if (request == null)
			{
				try
				{
					using (var reader = new StreamReader(req.Body))
					{
						request = JsonConvert.DeserializeObject<SwingData>(reader.ReadToEnd());
					}
				}
				catch { }
			}

			if (request == null)
			{
				return new BadRequestResult();
			}

			var data = new SwingDataEntity
			{
				PartitionKey = request.User,
				RowKey = $"{DateTimeOffset.MaxValue.Ticks - request.Time.Ticks:d19}_{Guid.NewGuid()}",
				User = request.User,
				Time = request.Time,
				Dump = request.Dump,
				Club = (int)request.Club,
				HeadSpeed = request.HeadSpeed,
				BallSpeed = request.BallSpeed,
				Distance = request.Distance,
				Meet = request.Meet
			};

			await dataTable.ExecuteAsync(TableOperation.InsertOrMerge(data));


			var tableQuery = new TableQuery<SwingLoggerEntity>();
			tableQuery.FilterString = TableQuery.CombineFilters(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0"),
				TableOperators.And,
				TableQuery.GenerateFilterCondition("DeviceId", QueryComparisons.Equal, request.User));
			var logger = (await loggerTable.ExecuteQuerySegmentedAsync(tableQuery, null)).FirstOrDefault();
			if (logger != null)
			{
				logger.IncomingData = true;
				await loggerTable.ExecuteAsync(TableOperation.Merge(logger));
			}

			return new OkResult();
		}
	}
}
