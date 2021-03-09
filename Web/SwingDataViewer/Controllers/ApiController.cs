using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SwingCommon;
using SwingDataViewer.Models;
using SwingDataViewer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwingDataViewer.Controllers
{
	[Authorize]
	public class ApiController : ControllerBase
	{
		TableService _tableService;
		ILogger _logger;

		public ApiController(TableService tableService, ILogger<AuthController> logger)
		{
			_tableService = tableService;
			_logger = logger;
		}

		public async Task<IActionResult> Ranking()
		{
			var user = UserModel.FromUserClaims(HttpContext.User);
			if (user == null) return Unauthorized();

			var targetTime = DateTime.UtcNow;
			var summary = await _tableService.GetSummary(targetTime);

			var result = new List<RankInformation>();

			#region TotalDistance
			var targetArray = summary.Where(s => s.DataType == SwingCommon.Enum.SummaryType.TotalDistance).OrderByDescending(s => s.Result).ToArray();
			var rank = Array.FindIndex(targetArray, a => a.DeviceId == user.DeviceId);
			if (rank >= targetArray.Length) rank = -1;
			result.Add(new RankInformation
			{
				Type = SwingCommon.Enum.SummaryType.TotalDistance,
				Rank = rank
			});
			#endregion

			#region MaxHeadSpeed
			targetArray = summary.Where(s => s.DataType == SwingCommon.Enum.SummaryType.MaxHeadSpeed).OrderByDescending(s => s.Result).ToArray();
			rank = Array.FindIndex(targetArray, a => a.DeviceId == user.DeviceId);
			if (rank >= targetArray.Length) rank = -1;
			result.Add(new RankInformation
			{
				Type = SwingCommon.Enum.SummaryType.MaxHeadSpeed,
				Rank = rank
			});
			#endregion

			#region MinHeadSpeed
			targetArray = summary.Where(s => s.DataType == SwingCommon.Enum.SummaryType.MinHeadSpeed).OrderBy(s => s.Result).ToArray();
			rank = Array.FindIndex(targetArray, a => a.DeviceId == user.DeviceId);
			if (rank >= targetArray.Length) rank = -1;
			result.Add(new RankInformation
			{
				Type = SwingCommon.Enum.SummaryType.MinHeadSpeed,
				Rank = rank
			});
			#endregion

			#region MaxMeetRate
			targetArray = summary.Where(s => s.DataType == SwingCommon.Enum.SummaryType.MaxMeetRate).OrderByDescending(s => s.Result).ToArray();
			rank = Array.FindIndex(targetArray, a => a.DeviceId == user.DeviceId);
			if (rank >= targetArray.Length) rank = -1;
			result.Add(new RankInformation
			{
				Type = SwingCommon.Enum.SummaryType.MaxMeetRate,
				Rank = rank
			});
			#endregion

			#region MinMeetRate
			targetArray = summary.Where(s => s.DataType == SwingCommon.Enum.SummaryType.MinMeetRate).OrderBy(s => s.Result).ToArray();
			rank = Array.FindIndex(targetArray, a => a.DeviceId == user.DeviceId);
			if (rank >= targetArray.Length) rank = -1;
			result.Add(new RankInformation
			{
				Type = SwingCommon.Enum.SummaryType.MinMeetRate,
				Rank = rank
			});
			#endregion

			#region MaxDistance
			targetArray = summary.Where(s => s.DataType == SwingCommon.Enum.SummaryType.MaxDistance).OrderByDescending(s => s.Result).ToArray();
			rank = Array.FindIndex(targetArray, a => a.DeviceId == user.DeviceId);
			if (rank >= targetArray.Length) rank = -1;
			result.Add(new RankInformation
			{
				Type = SwingCommon.Enum.SummaryType.MaxDistance,
				Rank = rank
			});
			#endregion

			#region MinDistance
			targetArray = summary.Where(s => s.DataType == SwingCommon.Enum.SummaryType.MinDistance).OrderBy(s => s.Result).ToArray();
			rank = Array.FindIndex(targetArray, a => a.DeviceId == user.DeviceId);
			if (rank >= targetArray.Length) rank = -1;
			result.Add(new RankInformation
			{
				Type = SwingCommon.Enum.SummaryType.MinDistance,
				Rank = rank
			});
			#endregion

			#region TotalBalls
			targetArray = summary.Where(s => s.DataType == SwingCommon.Enum.SummaryType.TotalBalls).OrderByDescending(s => s.Result).ToArray();
			rank = Array.FindIndex(targetArray, a => a.DeviceId == user.DeviceId);
			if (rank >= targetArray.Length) rank = -1;
			result.Add(new RankInformation
			{
				Type = SwingCommon.Enum.SummaryType.TotalBalls,
				Rank = rank
			});
			#endregion

			return new ContentResult { Content = JsonConvert.SerializeObject(result) };
		}
	}
}
