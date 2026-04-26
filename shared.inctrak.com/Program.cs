using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace inctrak.com
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception excp)
            {
                TryLogStartupException(excp);
                Console.WriteLine(excp);
            }
        }

        private static void TryLogStartupException(Exception excp)
        {
            string path = Environment.GetEnvironmentVariable("AppSettings__ErrorLogPath");
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Path.Combine("logs", "errors.log");
            }

            string directory = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            File.AppendAllText(path, $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}] startup_exception{Environment.NewLine}{excp}{Environment.NewLine}");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
