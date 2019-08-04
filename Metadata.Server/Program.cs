using Microsoft.AspNetCore.Hosting;
using System;
using System.Configuration;

namespace Zhichkin.Metadata.Server
{
    public sealed class Program
    {
        private const string Setting_ServerURL = "ServerURL";

        public static void Main(string[] args)
        {
            string url = ConfigurationManager.AppSettings[Setting_ServerURL];

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(url)
                .UseStartup<Startup>()
                .Build();
            host.Run();
        }

        public static string GetErrorText(Exception ex)
        {
            string errorText = string.Empty;
            Exception error = ex;
            while (error != null)
            {
                errorText += (errorText == string.Empty) ? error.Message : Environment.NewLine + error.Message;
                error = error.InnerException;
            }
            return errorText;
        }
    }
}
