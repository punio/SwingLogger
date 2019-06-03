﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace SwingDataViewer.Entities
{
	public class SwingLoggerEntity : TableEntity
	{
		public string DeviceId { get; set; }

		public string Id { get; set; }
		public string Salt { get; set; }
		public string Password { get; set; }
		public bool IncomingData { get; set; }
	}
}