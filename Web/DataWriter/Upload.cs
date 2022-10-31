using Azure.Data.Tables;
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
using SwingCommon.Entities;
using SwingCommon.Function;

namespace DataWriter
{
	public class Upload
	{
		private readonly ILogger _logger;
		private readonly TableServiceClient _tableServiceClient;

		public Upload(TableServiceClient tableServiceClient, ILogger<Upload> logger)
		{
			_logger = logger;
			_tableServiceClient = tableServiceClient;
		}

		[FunctionName("Upload")]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
		{
			SwingData request = null;
			if (req.Headers.TryGetValue("Content-Encoding", out var value))
			{
				if (value.Any(c => c == "gzip"))
				{
					await using var decompressionStream = new GZipStream(req.Body, CompressionMode.Decompress);
					using var reader = new StreamReader(decompressionStream);
					request = JsonConvert.DeserializeObject<SwingData>(await reader.ReadToEndAsync());
				}
			}
			if (request == null)
			{
				try
				{
					using var reader = new StreamReader(req.Body);
					request = JsonConvert.DeserializeObject<SwingData>(await reader.ReadToEndAsync());
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
				TimeOffset = request.Time.Offset.ToString(),
				Dump = request.Dump,
				Club = (int)request.Club,
				HeadSpeed = request.HeadSpeed,
				BallSpeed = request.BallSpeed,
				Distance = request.Distance,
				Meet = request.Meet,
				Tag = request.Tag
			};

			await SwingDataEntity.UpsertAsync(_tableServiceClient, data);


			var logger = await (await SwingLoggerEntity.QueryAsync(_tableServiceClient, x => x.PartitionKey == "0" && x.RowKey == request.User)).FirstOrDefault();
			if (logger != null)
			{
				logger.IncomingData = true;
				logger.ETag = Azure.ETag.All;
				await SwingLoggerEntity.UpsertAsync(_tableServiceClient, logger);
			}

			return new OkResult();
		}
	}
}
