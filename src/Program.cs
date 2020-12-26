using System;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevopsTicketHelper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args == null)
                Environment.Exit(2);
            bool isNumber = int.TryParse(args[0], out int buildNumber);
            if (!isNumber)
                Environment.Exit(2);
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var manager = serviceProvider.GetService<Functions>();
            if (Convert.ToInt32(args[0]) > 0)
                manager.CreateTestItemsFromBuildId(buildNumber).GetAwaiter().GetResult();
            else
                Console.WriteLine("Error: Current build not found");
        }
        private static void ConfigureServices(IServiceCollection services)
        {
            var personalaccesstoken = "yourtoken";
            var base64pat = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", string.Empty,
                personalaccesstoken)));
            services.AddHttpClient("devops", c =>
            {
                c.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64pat);
            });
            services.AddLogging(configure => configure.AddConsole())
                .AddTransient<Functions>();
        }
    }
}