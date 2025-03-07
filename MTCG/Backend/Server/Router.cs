using System;
using System.Text.Json;
using System.Threading.Tasks;
using MTCG.Backend.Server;

namespace MTCG.Backend.Server
{
    public class Router
    {
        private Controller _controller = new Controller();


        
        public async Task<HttpResponse> HandleRequest(string method, string path, string body, string auth)
        {
            switch (method)
            {
                case "GET":
                    return await GetHandlerAsync(path, body, auth);

                case "POST":
                    return await PostHandlerAsync(path, body, auth);

                case "PUT":
                    return await PutHandlerAsync(path, body, auth);  
                case "DELETE":
                    return await DeleteHandlerAsync(path, body, auth);

                default:
                    return new HttpResponse
                    {
                        status = 405,
                        message = "Method Not Allowed",
                        body = JsonSerializer.Serialize(new { message = "405 Method Not Allowed" })
                    };
            }
        }

      
        private async Task<HttpResponse> GetHandlerAsync(string path, string body, string auth)
        {
            switch (path)
            {
                
                case "/cards":
                    return await _controller.ShowCardsOfUser(auth);

                case "/deck":
                    return await _controller.ShowDeckOfUser(auth);

                case "/deck?format=plain":
                    return await _controller.ShowDeckOfUserPlain(auth);

                case string stringpath when path.StartsWith("/users/"):
                    return await _controller.UserdataShow(body, auth, path);

                case "/stats":
                    return await _controller.UserStatsShow(body, auth);

                case "/scoreboard":
                    return await _controller.ScoreCheck();

                case "/tradings":
                    return await _controller.TradingList(); 

                case "/daily":
                    return await _controller.UpgradeRandomCard(auth);

                default:

                    return new HttpResponse
                    {
                        status = 404,
                        message = "Not Found",
                        body = JsonSerializer.Serialize(new { message = "404 Not Found" })
                    };
            }

            
        }

       
        private async Task<HttpResponse> PostHandlerAsync(string path, string body, string auth)
        {
            switch (path)
            {
                case "/users":
                    return await _controller.Register(body);

                case "/sessions":
                    return await _controller.Login(body);

                case "/packages":
                    return await _controller.PackageCreation(body, auth); 
                case "/transactions/packages":
                    return await _controller.PackageOpen(auth);
                case "/tradings":
                    return await _controller.Trading(body, auth); 
                case string paths when path.StartsWith("/tradings/"):
                    return await _controller.HandleTrade(body, auth, path); 
                case "/battles":
                    return await _controller.Battle(auth); 
                default:
                    return new HttpResponse
                    {
                        status = 404,
                        message = "Not Found",
                        body = JsonSerializer.Serialize(new { message = "404 Not Found" })
                    };
            }
        }

        private async Task<HttpResponse> PutHandlerAsync(string path, string body, string auth)
        {
            switch (path)
            {
                case string stringpath when path.StartsWith("/users/"):
                    return await _controller.UpdateUserBio(body, auth, path);     //x
                case "/deck":
                    return await _controller.PutInDeck(body, auth);    //x
                default:
                    return new HttpResponse
                    {
                        status = 404,
                        message = "Not Found",
                        body = JsonSerializer.Serialize(new { message = "404 Not Found" })
                    };
            }
        }

        private async Task<HttpResponse> DeleteHandlerAsync(string path, string body, string auth)
        {
            switch (path)
            {
                case string stringpath when path.StartsWith("/tradings/"):
                    return await _controller.DeleteTrading(path, auth); 
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