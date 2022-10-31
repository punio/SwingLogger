using DataWriter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;

[assembly: FunctionsStartup(typeof(DataWriter.Startup))]
namespace DataWriter
{
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			var configuration = builder.GetContext().Configuration;
			builder.Services.AddAzureClients(builder =>
			{
				builder.AddTableServiceClient(configuration["AzureWebJobsStorage"]);
			});
		}
	}
}
