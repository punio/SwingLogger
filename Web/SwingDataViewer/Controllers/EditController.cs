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

			return View(viewModel);
		}

		[HttpPost]
		public async Task<SwingDataModel[]> Post([FromForm]DataRequestModel request)
		{
			var user = UserModel.FromUserClaims(HttpContext.User);
			if (user == null) return null;
			if (!DateTime.TryParseExact(request.From, "yyyyMMdd", null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var from)) return null;
			if (!DateTime.TryParseExact(request.To, "yyyyMMdd", null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var to)) return null;
			from = from.AddMinutes(request.Offset);
			to = to.AddMinutes(request.Offset);
			return await _tableService.GetSwingDatas(user.DeviceId, from, to);
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
			var allData = await _tableService.GetSwingDatas(user.DeviceId, DateTime.MinValue, DateTime.MaxValue.AddDays(-2));

			var memoryStream = new MemoryStream();
			var resultArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
			var entry = resultArchive.CreateEntry("swing.csv");
			await using (var stream = entry.Open())
			{
				await using var writer = new StreamWriter(stream, Encoding.UTF8);
				writer.WriteLine("日時,クラブ,ヘッドスピード,ボールスピード,飛距離,ミート率,タグ");
				foreach (var data in allData)
				{
					writer.WriteLine($"{data.DateTime},{data.Club},{data.HeadSpeed},{data.BallSpeed},{data.Distance},{data.Meet},{data.Tag}");
				}
			}

			resultArchive.Dispose();
			memoryStream.Flush();
			return File(memoryStream.ToArray(), "application/octet-stream", "swing.zip");
		}
	}
}
