using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                int port;
                var val = Environment.GetEnvironmentVariable("HTTP_PLATFORM_PORT");
                if (!int.TryParse(val, out port))
                {
                    if (args.Length < 1 || !int.TryParse(args[0], out port))
                    {
                        port = 8888;
                    }
                }

                var server = new HttpServer(port, new FileHttpRequestHandler
                {
                    BasePath = @"c:\temp"
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
            Console.Write("{0}Z ", DateTime.UtcNow.ToString("o").Split('.').First());
            Console.WriteLine(format, args);
        }
    }
}
