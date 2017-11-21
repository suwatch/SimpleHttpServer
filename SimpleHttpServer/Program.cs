using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    class Program
    {
        static string _logFile;

        static void Main(string[] args)
        {
            try
            {
                _logFile = Path.Combine(Directory.GetCurrentDirectory(), "SimpleHttpServer.log");

                int port;
                var val = Environment.GetEnvironmentVariable("HTTP_PLATFORM_PORT");
                if (!int.TryParse(val, out port))
                {
                    if (args.Length < 1 || !int.TryParse(args[0], out port))
                    {
                        port = 8888;
                    }
                }

                string basePath = Environment.GetEnvironmentVariable("HTTP_BASE_PATH");
                if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
                {
                    basePath = Directory.GetCurrentDirectory();
                }

                var logFile = Environment.GetEnvironmentVariable("HTTP_LOGFILE");
                if (!string.IsNullOrEmpty(logFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(logFile));
                    _logFile = logFile;
                }

                var server = new HttpServer(port, new FileHttpRequestHandler
                {
                    BasePath = basePath
                });

                server.Listen();
            }
            catch (Exception ex)
            {
                Trace(ex);
            }
        }

        public static void Trace(object obj)
        {
            Trace("{0}", obj);
        }

        public static void Trace(string format, params object[] args)
        {
            var strb = new StringBuilder();
            strb.AppendFormat("{0}Z ", DateTime.UtcNow.ToString("o").Split('.').First());
            strb.AppendFormat(format, args);

            var text = strb.ToString();
            Console.WriteLine(text);

            try
            {
                File.AppendAllLines(_logFile, new[] { text });
            }
            catch
            {
            }
        }
    }
}
