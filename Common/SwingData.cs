using System;

namespace SwingCommon
{
	public class SwingData
	{
		public string User { get; set; }

		public DateTimeOffset Time { get; set; }
		public TimeSpan TimeOffset { get; set; }	// DateTimeOffset -> JSONで時差が消えてしまうので・・

		public string Dump { get; set; }

		public ClubType Club { get; set; }
		public int HeadSpeed { get; set; }
		public int BallSpeed { get; set; }
		public int Distance { get; set; }
		public int Meet { get; set; }

		public string Tag { get; set; }
	}
}
