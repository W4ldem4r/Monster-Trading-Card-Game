using System;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;

namespace MTCG.Backend.Server
{
    public class HttpServer
    {
        private static TcpListener httpServer;

        public async Task StartServer()
        {
            // Start the HTTP-Server
           httpServer = new TcpListener(IPAddress.Loopback, 10001);
            httpServer.Start();
            Console.WriteLine("Server started on port 10001...");

            var router = new Router();

            while (true)
            {
                try
                {
                    TcpClient clientSocket = await httpServer.AcceptTcpClientAsync();

                    _ = Task.Run(() => HandleClient(clientSocket, router)); // _ = Task.Run(() => = standart mutlithreading                                                                      
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }
        private async Task HandleClient(TcpClient clientSocket, Router router)
        {
            using var writer = new StreamWriter(clientSocket.GetStream()) { AutoFlush = true };
            using var reader = new StreamReader(clientSocket.GetStream());

            // Read the HTTP-Request
            string? line = await reader.ReadLineAsync();
            if (line != null)
            {
                Console.WriteLine(line);
                var request = line.Split(' ');
                if (request.Length < 3)
                {
                    await writer.WriteLineAsync("HTTP/1.0 400 Bad Request\r\n");
                    return;
                }

                string method = request[0];
                string path = request[1];
                string auth = string.Empty;
                
                int contentLength = 0;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    Console.WriteLine(line);
                    if (line == "")
                    {
                        break;
                    }
                    var parts = line.Split(':');
                    if (parts.Length == 2 && parts[0].Trim() == "Authorization")
                    {
                        
                        string authHeader = parts[1].Trim();
                        if (authHeader.StartsWith("Bearer "))
                        {
                            // Nehme token raus und speichere ihn
                            auth = authHeader.Substring(7).Split('-')[0];
                            Console.WriteLine("Extracted Auth: " + auth);
                        }
                    }

                    if (parts.Length == 2 && parts[0] == "Content-Length")
                    {
                        contentLength = int.Parse(parts[1].Trim());
                    }
                }

              
                string requestBody = string.Empty;
                if (contentLength > 0)
                {
                    var data = new char[contentLength];
                    await reader.ReadAsync(data, 0, contentLength);
                    requestBody = new string(data);
                    Console.WriteLine(requestBody);
                }

             
                var response = await router.HandleRequest(method, path, requestBody,auth);

               Console.WriteLine("----------------------------------------");

               
                await writer.WriteLineAsync($"HTTP/1.1 {response.status} {response.message}\r\n");
                await writer.WriteLineAsync($"Content-Type: {response.type}; charset=utf-8\r\n");
                await writer.WriteLineAsync("\r\n");
                await writer.WriteLineAsync(response.body);

                Console.WriteLine("========================================");
            }
            else
            {
                await writer.WriteLineAsync("HTTP/1.0 400 Bad Request\r\n");
            }

            clientSocket.Close();
        }
    }



    public class StreamTracer : StreamWriter
    {
        public StreamTracer(StreamWriter writer) : base(writer.BaseStream)
        {

        }

        public override void WriteLine(string? value)
        {
            Console.WriteLine(value);
            base.WriteLine(value);
        }
    }
}


