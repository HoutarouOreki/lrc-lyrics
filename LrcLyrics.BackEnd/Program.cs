using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace LrcLyrics.BackEnd
{
    public class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseUrls("https://localhost:5001", "http://localhost:5000", "https://192.168.0.104:5001", "http://192.168.0.104:5000", "http://*:80", "http://*:81", "https://*:443");
                });
    }
}
