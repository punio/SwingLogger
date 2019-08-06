using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwingDataViewer.Services
{
	public static class DistanceFunction
	{
		public static string Abstract(string yards)
		{
			var yd = long.Parse(yards);
			// ヤードよりキロの方がいいかな
			var km = yd * 0.0009144;
			var rand = new Random();

			if (km < 34.5)
			{
				if (rand.Next(2) < 1) return $"スカイツリー {km / 0.634:f1}本分";       // 1本0.634km
				else return $"あべのハルカス {km / 0.3:f1}本分"; // 1本0.3km
			}

			if (km < 552.6)
			{
				if (rand.Next(2) < 1) return $"山手線 {km / 34.5:f1}周分";       // 1周34.5km
				else return $"大阪環状線 {km / 21.7:f1}本分"; // 1周21.7km 東京の山手線に対応するのは大阪環状線なのかな？
			}

			if (km < 6550) return $"東京-新大阪 {km / (552.6 * 2):f1}往復";  // 東京-新大阪 552.6km

			if (km < 384400) return $"東京-ハワイ {km / (6550.0 * 2):f1}往復"; // 東京-ハワイ(ホノルル) 6550.3km

			return $"地球-月 {km / (384400 * 2):f1}往復";        // 地球-月 384400km
		}
	}
}
