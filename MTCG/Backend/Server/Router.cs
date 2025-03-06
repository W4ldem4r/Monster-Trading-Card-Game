using System;
using System.Text.Json;
using System.Threading.Tasks;
using MTCG.Backend.Server;

namespace MTCG.Backend.Server
{
    public class Router
    {
        private readonly Controller _controller = new Controller();

        // Requests are split into GET and POST handlers
        public async Task<HttpResponse> HandleRequest(string method, string path, string body, string auth)
        {
            switch (method)
            {
                case "GET":
                    return await GetHandlerAsync(path, body, auth);
                case "POST":
                    return await PostHandlerAsync(path, body, auth);
                case "PUT":
                    return await PutHandlerAsync(path, body, auth);  //x                
                case "DELETE":
                    return await DeleteHandlerAsync(path, body, auth); //x
                    
                default:
                    return new HttpResponse
                    {
                        status = 405,
                        message = "Method Not Allowed",
                        body = JsonSerializer.Serialize(new { message = "405 Method Not Allowed" })
                    };
            }
        }
        
        // Sends a direct response back
        private async Task<HttpResponse> GetHandlerAsync(string path, string body, string auth)
        {
            switch (path)
            {
                ///so ein return am ende einer jeden funktion
                case "/cards":
                    return await _controller.ShowCardsOfUser(auth);
                case "/deck":
                    return await _controller.ShowDeckOfUser(auth);
                case "/deck?format=plain":
                    return await _controller.ShowDeckOfUserPlain(auth); 
                case string stringpath when path.StartsWith("/users/"): 
                    return await _controller.UserdataShow(body,auth, path); 
                case "/stats":
                  return await _controller.UserStatsShow(body,auth);
                case "/scoreboard":
                    return await _controller.ScoreCheck(); //x
                case "/tradings":
                //    return await _controller.PackageOpen(body); //x
                
                default:

                    return new HttpResponse
                    {
                        status = 404,
                        message = "Not Found",
                        body = JsonSerializer.Serialize(new { message = "404 Not Found" })
                    };
            }

            // await Task.Yield(); 

        }

        // POST requests are routed to corresponding functions in the controller
        private async Task<HttpResponse> PostHandlerAsync(string path, string body, string auth)
        {
            switch (path)
            {
                case "/users":
                    return await _controller.Register(body);
                case "/sessions":
                    return await _controller.Login(body);
                case "/packages":
                    return await _controller.PackageCreation(body, auth); //x
                case "/transactions/packages":
                    return await _controller.PackageOpen(auth); //x
                case string paths when path.StartsWith("/tradings/"):
                //    await HandleTrade(body,auth); //x                    
                case "/battles":
                //    await HandleBattleRequestAsync(body, auth); //x                 
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
                  return await _controller.PutInDeck(body,auth);    //x
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
                //        return await _controller.DeleteTrading(body);           //x
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
