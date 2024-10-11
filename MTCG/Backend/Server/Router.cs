using System;
using System.Text.Json;


namespace MTCG.Backend.Server
{
	public class Router
	{
        private readonly Controller _controller = new Controller();

        //request werden in get und post aufgeteilt
        public HttpResponse HandleRequest(string method, string path, string body)
        {
            switch (method)
            {
                case "GET":
                    return GetHandler(path);
                case "POST":
                    return PostHandler(path, body);
                default:
                    return new HttpResponse
                    {
                        status = 405,
                        message = "Method Not Allowed",
                        body = JsonSerializer.Serialize(new { message = "405 Method Not Allowed" })
                    };
            }
	}
        //schickt direkten response zurück
        private HttpResponse GetHandler(string path)
        {
            return new HttpResponse
            {
                status = 200,
                message = "OK",
                body = JsonSerializer.Serialize(new { message = "OK! (GET)" })
            };
        }

        //posts werden je nach pfad umgeleitet zu den entsprechenden funktionen in controller
        private HttpResponse PostHandler(string path, string body)
        {
            switch (path)
            {
                case "/users":
                    return _controller.Register(body);
                case "/sessions":
                    return _controller.Login(body);
                case "/packages":
                    return _controller.PackageCreation(body);
                default:
                    return new HttpResponse
                    {
                        status = 404,
                        message = "Not Found",
                        body = JsonSerializer.Serialize(new { message = "404 Not Found" })
                    };
            }
        }
    }
}