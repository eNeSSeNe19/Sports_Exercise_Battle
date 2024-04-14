namespace Sports_Exercise_Battle.Server.HTTP
{
    public class HttpResponse
    {
        private StreamWriter writer;

        public string HttpVersion { get; set; } = "HTTP/1.0";
        public int ResponseCode { get; set; } = 200;
        public string ResponseMessage { get; set; } = "OK";
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string? Content { get; set; }

        public HttpResponse(StreamWriter writer)
        {
            this.writer = writer;
        }

        public void Send()
        {
            var writerAlsoToConsole = new StreamTracer(writer);  // we use a simple helper-class StreamTracer to write the HTTP-Response to the client and to the console

            writerAlsoToConsole.WriteLine($"{HttpVersion} {ResponseCode} {ResponseMessage}");    // first line in HTTP-Response contains the HTTP-Version and the status code

            if ( Content != null)
            {
                Headers["Content-Length"] = Content.Length.ToString();
            }
            foreach (var header in Headers)
            {
                writerAlsoToConsole.WriteLine($"{header.Key}: {header.Value}");    // the HTTP-headers (in HTTP after the first line, until the empy line)
            }
            writerAlsoToConsole.WriteLine();
            if ( Content != null )
                writerAlsoToConsole.WriteLine(Content);    
        }
    }
}