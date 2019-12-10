using System;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DataWriter
{
	public static class Aggregate
	{
		[FunctionName("Aggregate")]
		public static async Task Run(
#if DEBUG
			[TimerTrigger("0 */1 * * * *")]TimerInfo myTimer,
#else
			[TimerTrigger("0 3 18 * * *")]TimerInfo myTimer,
#endif
			[DurableClient]IDurableOrchestrationClient starter,
			ILogger log)
		{
			var instanceId = await starter.StartNewAsync("AggregateOrchestrator", null);
			log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
		}
	}
}
