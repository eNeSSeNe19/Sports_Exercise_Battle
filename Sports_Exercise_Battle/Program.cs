using Sports_Exercise_Battle.Database;
using Sports_Exercise_Battle.Server.HTTP;
using Sports_Exercise_Battle.Server.SEB;
using System.Net;

Console.WriteLine("Our first simple HTTP-Server: http://localhost:10001/");

// ===== I. Start the HTTP-Server =====
HttpServer httpServer = new HttpServer(IPAddress.Any, 10001);
httpServer.RegisterEndpoint("users", new UsersEndpoint());
httpServer.RegisterEndpoint("sessions", new UsersEndpoint());
httpServer.RegisterEndpoint("stats", new UsersEndpoint());
httpServer.RegisterEndpoint("score", new UsersEndpoint());
httpServer.RegisterEndpoint("history", new UsersEndpoint());
httpServer.RegisterEndpoint("tournament", new UsersEndpoint());
httpServer.Run();

