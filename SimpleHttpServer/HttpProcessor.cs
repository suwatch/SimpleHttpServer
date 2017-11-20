using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    public interface IHttpRequestHandler
    {
        HttpResponse Handle(HttpRequest request);
    }

    public class HttpProcessor
    {
        //private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        private IHttpRequestHandler _handler;

        public HttpProcessor(IHttpRequestHandler handler)
        {
            _handler = handler;
        }

        public async Task HandleClient(TcpClient tcpClient)
        {
            Stream inputStream = GetInputStream(tcpClient);
            Stream outputStream = GetOutputStream(tcpClient);
            HttpRequest request = await GetRequest(inputStream, outputStream);
            Program.Trace(request);

            // route and handle the request...
            HttpResponse response = _handler.Handle(request);
            Program.Trace(response);

            await WriteResponse(outputStream, response);

            outputStream.Flush();
            outputStream.Close();
            outputStream = null;

            inputStream.Close();
            inputStream = null;

        }

        // this formats the HTTP response...
        private static async Task WriteResponse(Stream stream, HttpResponse response)
        {
            if (response.Content == null)
            {
                response.Content = new byte[] { };
            }

            // default to text/html content type
            if (!response.Headers.ContainsKey("Content-Type"))
            {
                response.Headers["Content-Type"] = "text/html";
            }

            response.Headers["Content-Length"] = response.Content.Length.ToString();

            await Write(stream, string.Format("HTTP/1.1 {0} {1}\r\n", response.StatusCode, response.ReasonPhrase));
            await Write(stream, string.Join("\r\n", response.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            await Write(stream, "\r\n\r\n");

            await stream.WriteAsync(response.Content, 0, response.Content.Length);
        }

        private static async Task<string> Readline(Stream stream)
        {
            byte[] buffer = new byte[1];
            int next_char;
            string data = "";
            while (true)
            {
                var read = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (read <= 0)
                    continue;
                next_char = buffer[0];
                //next_char = stream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }

        private static async Task Write(Stream stream, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        protected virtual Stream GetOutputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        protected virtual Stream GetInputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        private async Task<HttpRequest> GetRequest(Stream inputStream, Stream outputStream)
        {
            //Read Request Line
            string request = await Readline(inputStream);

            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            string method = tokens[0].ToUpper();
            string pathAndQuery = tokens[1];
            string protocolVersion = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string line;
            while ((line = await Readline(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);
            }

            string content = null;
            if (headers.ContainsKey("Content-Length"))
            {
                int totalBytes = Convert.ToInt32(headers["Content-Length"]);
                int bytesLeft = totalBytes;
                byte[] bytes = new byte[totalBytes];

                while (bytesLeft > 0)
                {
                    byte[] buffer = new byte[bytesLeft > 1024 ? 1024 : bytesLeft];
                    int n = inputStream.Read(buffer, 0, buffer.Length);
                    buffer.CopyTo(bytes, totalBytes - bytesLeft);

                    bytesLeft -= n;
                }

                content = Encoding.ASCII.GetString(bytes);
            }


            return new HttpRequest()
            {
                Method = method,
                Url = new Uri(string.Format("http://{0}{1}", headers["HOST"], pathAndQuery)),
                Headers = headers,
                Content = content
            };
        }
    }
}
