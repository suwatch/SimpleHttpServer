﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public Uri Url { get; set; } // either the Url, or the first regex group
        public string Content { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public HttpRequest()
        {
            this.Headers = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(this.Content))
                if (!this.Headers.ContainsKey("Content-Length"))
                    this.Headers.Add("Content-Length", this.Content.Length.ToString());

            return string.Format("{0} {1} HTTP/1.1\r\n{2}\r\n\r\n{3}", this.Method, this.Url.PathAndQuery, string.Join("\r\n", this.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))), this.Content);
        }
    }
}
