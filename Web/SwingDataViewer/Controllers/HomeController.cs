using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SwingCommon.Enum;
using SwingDataViewer.Entities;
using SwingDataViewer.Models;
using SwingDataViewer.Services;

namespace SwingDataViewer.Controllers
{
	public class HomeController : Controller
	{
		readonly TableService _tableService;
		ILogger _logger;

		public HomeController(TableService tableService, ILogger<HomeController> logger)
		{
			_tableService = tableService;
			_logger = logger;
		}

		public async Task<IActionResult> Index()
		{
			var viewModel = new HomeViewModel();

			var loggers = await _tableService.GetRegisterdLoggers();
			var allLoggers = loggers.Select(l => new User { Id = l.DeviceId, Name = (l.Public ? l.Name : "？？？"), Unknown = !l.Public }).ToArray();

			viewModel.Loggers = (await _tableService.GetRegisterdLoggers())
				.Where(l => l.IncomingData && l.Public)
				.Select(l => new User { Id = l.DeviceId, Name = l.Name })
				.ToArray();

			var targetTime = DateTime.UtcNow;
			var summary = await _tableService.GetSummary(targetTime);
			viewModel.ThisMonth = MakeRnakingData(allLoggers, summary);
			viewModel.ThisMonth.TargetDate = $"{targetTime:yyyy年MM月}";
			targetTime = targetTime.AddMonths(-1);
			summary = await _tableService.GetSummary(targetTime);
			viewModel.LastMonth = MakeRnakingData(allLoggers, summary);
			viewModel.LastMonth.TargetDate = $"{targetTime:yyyy年MM月}";

			return View(viewModel);
		}

		public IActionResult Start()
		{
			return View();
		}

		public IActionResult Graph(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				var user = UserModel.FromUserClaims(HttpContext.User);
				id = user?.DeviceId;
				if (!string.IsNullOrEmpty(id)) return RedirectToAction("Graph", new { id });
			}
			if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

			var viewModel = new GraphViewModel();
			viewModel.User = id;
			return View(viewModel);
		}

		public async Task<IActionResult> Statistics(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				var user = UserModel.FromUserClaims(HttpContext.User);
				id = user?.DeviceId;
				if (!string.IsNullOrEmpty(id)) return RedirectToAction("Statistics", new { id });
			}
			if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

			var model = new StatisticsModel();
			model.Data = await _tableService.GetStatistics(id);

			return View(model);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}


		private RankingData MakeRnakingData(User[] users, SwingSummaryEntity[] summary)
		{
			var result = new RankingData();

			// ユーザーでフィルタをはじめにやった方が速い？？

			#region TotalDistance
			result.TotalDistance = summary
				.Where(s => s.DataType == SummaryType.TotalDistance)
				.OrderByDescending(s => s.Result)
				.Select(s => new SummaryData
				{
					Value = s.Result.ToString(),
					User = users.FirstOrDefault(u => u.Id == s.DeviceId)
				})
				.Where(s => s.User != null)
				.Take(5)
				.ToArray();
			#endregion

			#region MaxHeadSpeed
			result.MaxHeadSpeed = summary
				.Where(s => s.DataType == SummaryType.MaxHeadSpeed)
				.OrderByDescending(s => s.Result)
				.Select(s => new SummaryData
				{
					Value = (s.Result / 10.0).ToString("f1"),
					User = users.FirstOrDefault(u => u.Id == s.DeviceId)
				})
				.Where(s => s.User != null)
				.Take(5)
				.ToArray();
			#endregion

			#region MinHeadSpeed
			result.MinHeadSpeed = summary
				.Where(s => s.DataType == SummaryType.MinHeadSpeed)
				.OrderBy(s => s.Result)
				.Select(s => new SummaryData
				{
					Value = (s.Result / 10.0).ToString("f1"),
					User = users.FirstOrDefault(u => u.Id == s.DeviceId)
				})
				.Where(s => s.User != null)
				.Take(5)
				.ToArray();
			#endregion

			#region MaxMeetRate
			result.MaxMeetRate = summary
				.Where(s => s.DataType == SummaryType.MaxMeetRate)
				.OrderByDescending(s => s.Result)
				.Select(s => new SummaryData
				{
					Value = (s.Result / 100.0).ToString("f2"),
					User = users.FirstOrDefault(u => u.Id == s.DeviceId)
				})
				.Where(s => s.User != null)
				.Take(5)
				.ToArray();
			#endregion

			#region MinMeetRate
			result.MinMeetRate = summary
				.Where(s => s.DataType == SummaryType.MinMeetRate)
				.OrderBy(s => s.Result)
				.Select(s => new SummaryData
				{
					Value = (s.Result / 100.0).ToString("f2"),
					User = users.FirstOrDefault(u => u.Id == s.DeviceId)
				})
				.Where(s => s.User != null)
				.Take(5)
				.ToArray();
			#endregion

			#region MaxDistance
			result.MaxDistance = summary
				.Where(s => s.DataType == SummaryType.MaxDistance)
				.OrderByDescending(s => s.Result)
				.Select(s => new SummaryData
				{
					Value = s.Result.ToString(),
					User = users.FirstOrDefault(u => u.Id == s.DeviceId)
				})
				.Where(s => s.User != null)
				.Take(5)
				.ToArray();
			#endregion

			#region MinDistance
			result.MinDistance = summary
				.Where(s => s.DataType == SummaryType.MinDistance)
				.OrderBy(s => s.Result)
				.Select(s => new SummaryData
				{
					Value = s.Result.ToString(),
					User = users.FirstOrDefault(u => u.Id == s.DeviceId)
				})
				.Where(s => s.User != null)
				.Take(5)
				.ToArray();
			#endregion

			#region TotalBalls
			result.TotalBalls = summary
				.Where(s => s.DataType == SummaryType.TotalBalls)
				.OrderByDescending(s => s.Result)
				.Select(s => new SummaryData
				{
					Value = s.Result.ToString(),
					User = users.FirstOrDefault(u => u.Id == s.DeviceId)
				})
				.Where(s => s.User != null)
				.Take(5)
				.ToArray();
			#endregion

			return result;
		}
	}
}
