using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
			if (id == "1") id = "CCF9DA6B-55FA-4039-A8B8-B93AC4D5B058";
			viewModel.User = id;
			return View(viewModel);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
