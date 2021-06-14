using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwingDataViewer.Models
{
	public static class SummaryFunction
	{
		public static string GetRankNumber(int index, SummaryData[] summaryDatas)
		{
			if (summaryDatas.Length <= index) return "";
			if (summaryDatas.Length < 1) return "";

			var target = summaryDatas[index].SortValue;

			return (summaryDatas.Count(d => d.SortValue > target) + 1).ToString();
		}
	}
}
