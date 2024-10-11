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
        public void Start()
        {
            // ===== I. Start the HTTP-Server =====
            var httpServer = new TcpListener(IPAddress.Loopback, 10001); 
            httpServer.Start();
            Console.WriteLine("Server started on port 10001...");

            var router = new Router();

            while (true)
            {
                // ----- 0. Accept the TCP-Client and create the reader and writer -----
                var clientSocket = httpServer.AcceptTcpClient();
                using var writer = new StreamWriter(clientSocket.GetStream()) { AutoFlush = true };
                using var reader = new StreamReader(clientSocket.GetStream());

                // ----- 1. Read the HTTP-Request -----
                string? line = reader.ReadLine();

                // 1.1 first line in HTTP contains the method, path and HTTP version
                if (line != null)
                {
                    Console.WriteLine(line);
                    var request = line.Split(' ');
                    if (request.Length < 3)
                    {
                        writer.WriteLine("HTTP/1.0 400 Bad Request");
                        writer.WriteLine();
                        continue;
                    }

                    string method = request[0];
                    string path = request[1];

                    // 1.2 read the HTTP-headers (in HTTP after the first line, until the empty line)
                    int content_length = 0; // we need the content_length later, to be able to read the HTTP-content
                    while ((line = reader.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                        if (line == "")
                        {
                            break;  // empty line indicates the end of the HTTP-headers
                        }

                        // Parse the header
                        var parts = line.Split(':');
                        if (parts.Length == 2 && parts[0] == "Content-Length")
                        {
                            content_length = int.Parse(parts[1].Trim());
                        }
                    }

                    // 1.3 read the body if existing
                    string requestBody = string.Empty;
                    if (content_length > 0)
                    {
                        var data = new StringBuilder(200);
                        char[] chars = new char[1024];
                        int bytesReadTotal = 0;
                        while (bytesReadTotal < content_length)
                        {
                            var bytesRead = reader.Read(chars, 0, chars.Length);
                            bytesReadTotal += bytesRead;
                            if (bytesRead == 0)
                                break;
                            data.Append(chars, 0, bytesRead);
                        }
                        requestBody = data.ToString();
                        Console.WriteLine(requestBody);
                    }

                    // ----- 2. Do the processing -----
                    var response = router.HandleRequest(method, path, requestBody);

                    Console.WriteLine("----------------------------------------");

                    // ----- 3. Write the HTTP-Response -----
                    writer.WriteLine($"HTTP/1.1 {response.status} {response.message}");    
                    writer.WriteLine("Content-Type: application/json; charset=utf-8");    
                    writer.WriteLine();
                    writer.WriteLine(response.body);    

                    Console.WriteLine("========================================");
                }
                else
                {
                    writer.WriteLine("HTTP/1.0 400 Bad Request");
                    writer.WriteLine();
                }

                // Socket connection wird geschlossen
                clientSocket.Close();
            }
        }
    }

    //Optionaler StreamTracer zum späteren Debugging
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


