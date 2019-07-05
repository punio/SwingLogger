using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwingDataViewer.Models;
using SwingDataViewer.Services;

namespace SwingDataViewer.Controllers
{
	[Authorize]
	public class EditController : Controller
	{
		readonly TableService _tableService;
		public EditController(TableService tableService)
		{
			_tableService = tableService;
		}

		public async Task<IActionResult> Index()
		{
			var user = UserModel.FromUserClaims(HttpContext.User);
			if (user == null) return RedirectToAction("Login", "Auth", new { returnUrl = "~/edit/" });

			var viewModel = new EditViewModel();
			viewModel.SwingData = await _tableService.GetSwingDatasAsync(user.DeviceId);

			return View(viewModel);
		}

		[HttpDelete]
		public async Task<IActionResult> Delete(string id)
		{
			var user = UserModel.FromUserClaims(HttpContext.User);
			if (user == null) return Unauthorized();

			await _tableService.DeleteSwingData(user.DeviceId, id);
			return Ok();
		}

		[HttpGet]
		public async Task<IActionResult> DownloadData()
		{
			var user = UserModel.FromUserClaims(HttpContext.User);
			if (user == null) return RedirectToAction("Login", "Auth", new { returnUrl = "~/edit/" });
			var allData = await _tableService.GetSwingDatasAsync(user.DeviceId);

			var memoryStream = new MemoryStream();
			var resultArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
			var entry = resultArchive.CreateEntry("swing.csv");
			using (var stream = entry.Open())
			using (var writer = new StreamWriter(stream, Encoding.UTF8))
			{
				writer.WriteLine("日時,クラブ,ヘッドスピード,ボールスピード,飛距離,ミート率");
				foreach (var data in allData)
				{
					writer.WriteLine($"{data.DateTime},{data.Club},{data.HeadSpeed},{data.BallSpeed},{data.Distance},{data.Meet}");
				}
			}
			resultArchive.Dispose();
			memoryStream.Flush();
			return File(memoryStream.ToArray(), "application/octet-stream", "swing.zip");
		}
	}
}
