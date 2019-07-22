using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace SwingDataViewer.Entities
{
	public class SwingDataEntity : TableEntity
	{
		public string User { get; set; }

		public DateTimeOffset Time { get; set; }
		public string TimeOffset { get; set; }    // DateTimeOffset -> JSONで時差が消えてしまうので・・
		public string Dump { get; set; }

		public int Club { get; set; }
		public int HeadSpeed { get; set; }
		public int BallSpeed { get; set; }
		public int Distance { get; set; }
		public int Meet { get; set; }

		public string Tag { get; set; }



		private DateTimeOffset _localTime = DateTimeOffset.MinValue;
		[JsonIgnore]
		public DateTimeOffset LocalTime
		{
			get
			{
				if (_localTime > DateTimeOffset.MinValue) return _localTime;
				_localTime = TimeSpan.TryParse(TimeOffset, out var offset) ? Time.DateTime.Add(offset) : Time;
				return _localTime;
			}
		}
	}
}
