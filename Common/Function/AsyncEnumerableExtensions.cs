using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwingCommon.Function
{
	public static class AsyncEnumerableExtensions
	{
		public static async Task<T> FirstOrDefault<T>(this IAsyncEnumerable<T> asyncEnumerable) where T : class
		{
			await foreach (var item in asyncEnumerable)
			{
				return item;
			}
			return default;
		}
	}
}
