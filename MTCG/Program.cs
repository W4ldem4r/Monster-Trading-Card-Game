using MTCG.Backend.Server;

namespace MTCG
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //Server starts
            var server = new HttpServer();
            await server.StartServer();
           
        }
    }
}