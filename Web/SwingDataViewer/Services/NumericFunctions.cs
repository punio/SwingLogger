using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwingDataViewer.Services
{
	public class NumericFunctions
	{
		/// <summary>
		/// モジュラ逆数を求めるよ
		/// </summary>
		/// <param name="a"></param>
		/// <param name="m">法</param>
		/// <returns></returns>
		private static int ModularInverse(int a, int m)
		{
			int i = m, v = 0, d = 1;
			while (a > 0)
			{
				int t = i / a, x = a;
				a = i % x;
				i = x;
				x = d;
				d = v - t * x;
				v = x;
			}
			v %= m;
			if (v < 0) v = (v + m) % m;
			return v;
		}

		/// <summary>
		/// 6桁の数値を作ろう
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int Scramble(int value)
		{
			const int odd = 123321;   // 奇数1
			var temp = (ulong)value * odd;
			temp %= 1000000;

			// 逆順
			temp = (temp / 1000) + (temp % 1000) * 1000;

			// 奇数1のモジュラ逆数をかける
			//temp = (uint)(temp * ModularInverse(odd, 1000000));
			temp = (ulong)(temp * 891081);
			temp %= 1000000;

			return (int)temp;
		}
	}
}
