﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using SwingCommon;

namespace DataWriter.Entities
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
	}
}
