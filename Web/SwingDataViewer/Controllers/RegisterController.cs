using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SwingCommon;
using SwingDataViewer.Models;
using SwingDataViewer.Services;

namespace SwingDataViewer.Controllers
{
	public class RegisterController : Controller
	{
		readonly TableService _tableService;

		public RegisterController(TableService tableService)
		{
			_tableService = tableService;
		}

		public IActionResult Index()
		{
			return View(new RegisterViewModel { Name = "匿名" });
		}

		[HttpPost]
		public async Task<IActionResult> Index(RegisterViewModel viewModel)
		{
			var result = await _tableService.RegisterNewId(viewModel);

			viewModel.HaveResult = true;
			viewModel.Error = string.IsNullOrEmpty(result);
			if (viewModel.Error)
			{
				viewModel.Message = "登録に失敗しました。すでに使われているIDかもしれません。";
			}
			else
			{
				viewModel.Message = $"登録完了 (ID={result})";
			}
			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> DeviceId([FromBody]RegisterViewModel viewModel)
		{
			var logger = await _tableService.GetLogger(viewModel.Id, viewModel.Password);
			var result = new RegisteredDevice
			{
				Find = logger != null,
				Id = logger?.DeviceId,
				Name = logger?.Name
			};

			return new JsonResult(result);
		}
	}
}