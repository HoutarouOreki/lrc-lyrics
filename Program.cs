using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace LrcLyrics
{
    public class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>().UseUrls("https://localhost:5001", "http://localhost:5000", "http://*:81", "https://*:444"));
        }
    }
}
