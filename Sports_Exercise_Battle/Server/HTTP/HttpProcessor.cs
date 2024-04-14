using System.Net.Sockets;
using System.Text;

namespace Sports_Exercise_Battle.Server.HTTP
{
    public class HttpProcessor
    {
        private TcpClient clientSocket;
        private HttpServer httpServer;


        public HttpProcessor(HttpServer httpServer, TcpClient clientSocket)
        {
            this.httpServer = httpServer;
            this.clientSocket = clientSocket;
        }

        public void Process()
        {

            // ----- 1. Read the HTTP-Request -----
            using var reader = new StreamReader(clientSocket.GetStream());
            var rq = new HttpRequest(reader);
            rq.Parse();

            // ----- 2. Do the processing -----
            using var writer = new StreamWriter(clientSocket.GetStream()) { AutoFlush = true };
            var rs = new HttpResponse(writer);

            var endpoint = httpServer.Endpoints.ContainsKey(rq.Path[1]) ? httpServer.Endpoints[rq.Path[1]] : null;
            if ( endpoint==null || !endpoint.HandleRequest(rq, rs) )
            {
                //Thread.Sleep(10000);
                rs.ResponseCode = 404;
                rs.ResponseMessage = "Not Found";
                rs.Content = "<html><body>Not found!</body></html>";
                rs.Headers.Add("Content-Type", "text/html");
            }

            Console.WriteLine("----------------------------------------");
            // ----- 3. Send the HTTP-Response -----
            rs.Send();
            writer.Flush();

            Console.WriteLine("========================================");
        }
    }
}