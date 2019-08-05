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

		[Route("{id}")]
		public async Task<TotalData> Get(string id)
		{
			var result = new TotalData();
			result.Swing = (await _tableService.GetSwingDatas(id)).OrderBy(c => c.ClubType).ThenBy(c => c.Date).ToArray();
			result.Date = result.Swing.Select(s => s.Date).Distinct().OrderBy(d => d).ToArray();
			return result;
		}
	}
}