﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
			viewModel.Loggers = (await _tableService.GetRegisterdLoggers())
				.Where(l => l.IncomingData && l.Public)
				.Select(l => new User { Id = l.DeviceId, Name = l.Name })
				.ToArray();

			var targetTime = DateTime.UtcNow;
			var summary = await _tableService.GetSummary(targetTime);
			viewModel.ThisMonth = MakeRnakingData(viewModel.Loggers, summary);
			viewModel.ThisMonth.TargetDate = $"{targetTime:yyyy年MM月}";
			targetTime = targetTime.AddMonths(-1);
			summary = await _tableService.GetSummary(targetTime);
			viewModel.LastMonth = MakeRnakingData(viewModel.Loggers, summary);
			viewModel.LastMonth.TargetDate = $"{targetTime:yyyy年MM月}";

			return View(viewModel);
		}

		public async Task<IActionResult> Graph(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				var user = UserModel.FromUserClaims(HttpContext.User);
				id = user?.DeviceId;
			}
			if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

			var viewModel = new GraphViewModel();
			viewModel.User = id;
			return View(viewModel);
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
				.Where(s => s.DataType == SwingSummaryEntity.SummaryType.TotalDistance)
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
				.Where(s => s.DataType == SwingSummaryEntity.SummaryType.MaxHeadSpeed)
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
				.Where(s => s.DataType == SwingSummaryEntity.SummaryType.MinHeadSpeed)
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
				.Where(s => s.DataType == SwingSummaryEntity.SummaryType.MaxMeetRate)
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
				.Where(s => s.DataType == SwingSummaryEntity.SummaryType.MinMeetRate)
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
				.Where(s => s.DataType == SwingSummaryEntity.SummaryType.MaxDistance)
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
				.Where(s => s.DataType == SwingSummaryEntity.SummaryType.MinDistance)
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
				.Where(s => s.DataType == SwingSummaryEntity.SummaryType.TotalBalls)
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
