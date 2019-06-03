using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace DataWriter
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
