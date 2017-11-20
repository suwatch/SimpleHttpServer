using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    public class HttpServer
    {

        protected int port;
        protected IHttpRequestHandler handler;
        TcpListener listener;
        bool is_active = true;

        public HttpServer(int port, IHttpRequestHandler handler)
        {
            this.port = port;
            this.handler = handler;
        }

        public void Listen()
        {
            listener = new TcpListener(IPAddress.Loopback, port);
            Program.Trace("Listening at {0}:{1}", IPAddress.Loopback, port);
            listener.Start();
            while (is_active)
            {
                TcpClient client = listener.AcceptTcpClient();
                HttpProcessor processor = new HttpProcessor(this.handler);
#pragma warning disable 4014
                processor.HandleClient(client);
#pragma warning restore 4014
            }
        }
    }
}
