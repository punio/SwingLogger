using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwingCommon;

namespace SwingDataViewer.Models
{
	public class HomeViewModel
	{
		public User[] Loggers { get; set; }

		public RankingData ThisMonth { get; set; }
		public RankingData LastMonth { get; set; }
	}

	public class User
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}


	public class RankingData
	{
		public string TargetDate { get; set; }
		public SummaryData[] TotalDistance { get; set; }
		public SummaryData[] MaxHeadSpeed { get; set; }
		public SummaryData[] MinHeadSpeed { get; set; }
		public SummaryData[] MaxMeetRate { get; set; }
		public SummaryData[] MinMeetRate { get; set; }
		public SummaryData[] MaxDistance { get; set; }
		public SummaryData[] MinDistance { get; set; }
		public SummaryData[] TotalBalls { get; set; }
	}

	public class SummaryData
	{
		public string Value { get; set; }
		public User User { get; set; }
	}
}
