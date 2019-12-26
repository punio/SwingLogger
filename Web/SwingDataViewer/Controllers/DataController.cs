using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SwingDataViewer.Models;
using SwingDataViewer.Services;

namespace SwingDataViewer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DataController : ControllerBase
	{
		TableService _tableService;
		ILogger _logger;
		public DataController(TableService tableService, ILogger<DataController> logger)
		{
			_tableService = tableService;
			_logger = logger;
		}

		[HttpPost]
		public async Task<TotalData> Graph([FromForm]DataRequestModel request)
		{
			var result = new TotalData();
			if (!DateTime.TryParseExact(request.From, "yyyyMMdd", null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var from)) return result;
			if (!DateTime.TryParseExact(request.To, "yyyyMMdd", null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var to)) return result;
			from = from.AddMinutes(request.Offset);
			to = to.AddMinutes(request.Offset);
			result.Swing = (await _tableService.GetSwingDatas(request.Id, from, to)).OrderBy(c => c.ClubType).ThenBy(c => c.Date).ToArray();
			result.Date = result.Swing.Select(s => s.Date).Distinct().OrderBy(d => d).Select(d => new Date(d)).ToArray();
			return result;
		}
	}
}