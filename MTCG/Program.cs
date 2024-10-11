using MTCG.Backend.Server;

namespace MTCG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Server wird gestartet
            var server = new HttpServer();
            server.Start();
        }
    }
}