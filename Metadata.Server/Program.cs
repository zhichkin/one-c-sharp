using Microsoft.AspNetCore.Hosting;
using System;

namespace Zhichkin.Metadata.Server
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(@"http://localhost:5000")
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
