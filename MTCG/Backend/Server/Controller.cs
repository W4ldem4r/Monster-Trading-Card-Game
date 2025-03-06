using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace MTCG.Backend.Server
{
    //strukturen des HttpResonse, User und Package KLasse
    public class HttpResponse
    {
        public int status { get; set; }
        public string message { get; set; }
        public string body { get; set; }
        public string type { get; set; } = "application/json";
    }



    public class Controller
    {
        //speichern der daten in dictionaries und lists
        private Dictionary<string, User> _users = new Dictionary<string, User>();
        private List<Package> _packages = new List<Package>();
        public DatabaseClass Database = new DatabaseClass();
        //user wird registriert und in memory abgespeichert
        public async Task<HttpResponse> Register(string body)
        {
            var user = JsonSerializer.Deserialize<User>(body);

            if (user == null || _users.ContainsKey(user.Username))
            {

                return new HttpResponse
                {
                    status = 400,
                    message = "Bad Request",
                    body = JsonSerializer.Serialize(new { message = "User already exists or invalid data" })
                };
            }

            _users[user.Username] = user;
            Console.WriteLine("User being registered");
            return new HttpResponse
            {
                status = 201,
                message = "Created",
                body = JsonSerializer.Serialize(new { message = "User created HTTP 201" })
            };
        }
        //username und passwort werden mit abgespeicherten daten verglichen und es wird session token erstellt
        public async Task<HttpResponse> Login(string body)
        {
            var user = JsonSerializer.Deserialize<User>(body);
            if (user == null || !_users.ContainsKey(user.Username) || _users[user.Username].Password != user.Password)
            {
                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = JsonSerializer.Serialize(new { message = "Login failed" })
                };
            }

            var token = $"{user.Username}-mtcgToken";

            return new HttpResponse
            {
                status = 200,
                message = "OK",
                body = JsonSerializer.Serialize(new { token })
            };
        }
        //package wird erstellt und in memory abgespeichert
        public async Task<HttpResponse> PackageCreation(string body, string auth)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new CardsConverter() }
            };

            var cards = JsonSerializer.Deserialize<List<Cards>>(body, options);


            Console.WriteLine(cards);


            if (cards == null)
            {
                return new HttpResponse
                {
                    status = 400,
                    message = "Bad Request",
                    body = JsonSerializer.Serialize(new { message = "Invalid package data" })
                };
            }
            else {
                if (auth == "admin")
                {
                    Package newpackage = new Package(cards);
                    _packages.Add(newpackage);
                    return new HttpResponse
                    {
                        status = 201,
                        message = "Created",
                        body = JsonSerializer.Serialize(new { message = "Packages created" })
                    };

                }
                else
                {
                    return new HttpResponse
                    {
                        status = 402,
                        message = "Unauthorized",
                        body = JsonSerializer.Serialize(new { message = "Unautharised for package creation" })
                    };
                }
            }
        }

        public async Task<User?> AuthChecker(string auth)
        {
            if (_users.TryGetValue(auth, out User user))
            {
                return await Task.FromResult(user);
            }

            return null;
        }

        public void SaveUser(User ModifiedUser) //Datenbank fehlt
        {

            if (_users.ContainsKey(ModifiedUser.Username))
            {
                _users[ModifiedUser.Username] = ModifiedUser;
            }
            else
            {
                _users.Add(ModifiedUser.Username, ModifiedUser);
            }

        }




        public async Task<HttpResponse> ShowCardsOfUser(string auth)
        {
            User? User = await AuthChecker(auth);
            if (User == null)
            {
                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = JsonSerializer.Serialize(new { message = "Login failed" })
                };
            }
            else
            {
                return new HttpResponse
                {
                    status = 201,
                    message = "User Found",
                    body = JsonSerializer.Serialize(new { message = "Cards in inventory", cards = User.Cards })


                };

            }


        }

        public async Task<HttpResponse> ShowDeckOfUser(string auth)
        {
            User? User = await AuthChecker(auth);
            if (User == null)
            {
                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = JsonSerializer.Serialize(new { message = "Login failed" })
                };
            }
            else
            {
                return new HttpResponse
                {
                    status = 201,
                    message = "User Found",

                     body = JsonSerializer.Serialize(new { message = "Cards in Deck", decks = User.Deck })

                };

            }


        }











        public async Task<HttpResponse> ShowDeckOfUserPlain(string auth)
        {
            User? User = await AuthChecker(auth);
            if (User == null)
            {
                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = "Login failed"
                };
            }
            else
            {
                string plainResponse = string.Join("\n", User.Deck.Select(card => $"{card.Name} (Damage: {card.Damage})"));

                return new HttpResponse
                {
                    status = 201,
                    message = "User Found",
                    body = plainResponse,
                    type = "text/plain"
                };
            }
        }


        public async Task<HttpResponse> ScoreCheck()
        {
            var UserSort = _users.Values.OrderByDescending(user => user.Elo).ToList();
            var Scoreboard = "";
            int rank = 1;

            foreach (var user in _users.Values.OrderByDescending(user => user.Elo))
            {
                Scoreboard += rank + ". " + user.Username + " - Elo: " + user.Elo + ". ";
                rank++;
            }

            return new HttpResponse
            {
                status = 200,
                message = "OK!",
                body = JsonSerializer.Serialize(new { message = Scoreboard })

            };
        }



        public async Task<HttpResponse> UserdataShow(string body, string auth, string path)
        {
            
            string userIdString = path.Substring(7);
            

            User? User = await AuthChecker(auth);
            if (User == null)
            {
                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = JsonSerializer.Serialize(new { message = "Login failed" })
                };
            }

            else if ( userIdString != User.Username)
            {
                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = JsonSerializer.Serialize(new { message = "Unauthorized Access" })
                };
            }


            else
            {
                return new HttpResponse
                {
                    status = 201,
                    message = "User Found",
                    body = JsonSerializer.Serialize(new { message = $"Userdata: User ID: {User.ID}, Username: {User.Username}, Bio: {User.Bio}, Image: {User.Image}, Name: {User.Name}, Coins: {User.Coins} " })

                };

            }
        }

        public async Task<HttpResponse> UpdateUserBio(string body, string auth, string path)
        {
            string userIdString = path.Substring(7);
            User? user = await AuthChecker(auth);
            if (user == null)
            {
                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = JsonSerializer.Serialize(new { message = "User not found" })
                };
            }
            else if (userIdString != user.Username)
            {
                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = JsonSerializer.Serialize(new { message = "Unauthorized Access" })
                };
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return new HttpResponse
                {
                    status = 400,
                    message = "Bad Request",
                    body = JsonSerializer.Serialize(new { message = "Body empty" })
                };
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("Name", out var name)) user.Name = name.GetString();
            if (root.TryGetProperty("Bio", out var bio)) user.Bio = bio.GetString();
            if (root.TryGetProperty("Image", out var image)) user.Image = image.GetString();

            SaveUser(user);

            return new HttpResponse
            {
                status = 200,
                message = "OK",
                body = JsonSerializer.Serialize(new { message = "User data changed" })
            };
        }



        public async Task<HttpResponse> UserStatsShow(string body, string auth)
        {
            User? User = await AuthChecker(auth);
            if (User == null)
            {
                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = JsonSerializer.Serialize(new { message = "Login failed" })
                };
            }
            else
            {
                return new HttpResponse
                {
                    status = 201,
                    message = "User Found",
                    body = JsonSerializer.Serialize(new { message = $"UserStats: Elo:{User.Elo}, Wins:{User.Wins}, Losses: {User.Losses}, Coins:{User.Coins}" })

                };

            }

        }


        public async Task<HttpResponse> PackageOpen(string auth)
        {


            User? User = await AuthChecker(auth);
            if (User == null)
            {
                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = JsonSerializer.Serialize(new { message = "Login failed" })
                };
            }
            else
            {

                if (_packages.Any())
                {

                    if (User.Coins >= 5)
                    {
                        var firstPackage = _packages.First();
                        User.Cards.AddRange(firstPackage.allCards);
                        _packages.RemoveAt(0);
                        User.Coins -= 5;
                        SaveUser(User);

                        return new HttpResponse
                        {
                            status = 201,
                            message = "Successfully Bought Cards",
                            body = JsonSerializer.Serialize(new { message = firstPackage.allCards })

                        };
                    }
                    else
                    {
                        return new HttpResponse
                        {
                            status = 404,
                            message = "Not enough Money",
                            body = JsonSerializer.Serialize(new { message = $"You have {User.Coins} Coins left" })

                        };
                    }
                }

                else
                {
                    return new HttpResponse
                    {
                        status = 404,
                        message = "No packages available",
                        body = JsonSerializer.Serialize(new { message = "No packages available" })
                    };
                }
                return new HttpResponse
                {
                    status = 201,
                    message = "User Found",
                    body = JsonSerializer.Serialize(new { message = $"Cards in inventory = {JsonSerializer.Serialize(User.Cards)}" })

                };

            }
        }

  
        public async Task<HttpResponse> PutInDeck(string body,string auth)
        {

            var User = await AuthChecker(auth);

            if(User == null)
            {

                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = JsonSerializer.Serialize(new { message = "Login failed" })
                };
            }
            else
            {
                var ID = JsonSerializer.Deserialize<List<Guid>>(body);
                if (ID == null)
                {
                    return new HttpResponse
                    {
                        status = 400,
                        message = "Bad Request",
                        body = JsonSerializer.Serialize(new { message = "No Card ID" })
                    };
                }
                else if (ID.Count < 4)
                { 
                    return new HttpResponse
                    {
                        status = 400,
                        message = "Bad Request",
                        body = JsonSerializer.Serialize(new { message = "You need atleast 4 Cards" })
                    };
                }
                else
                {
                    foreach (var id in ID)
                    {
                        var card = User.Cards.Find(card => card.Id == id);
                        if (card != null)
                        {
                            User.Deck.Add(card);

                        }
                        else
                        {
                            return new HttpResponse
                            {
                                status = 404,
                                message = "Card not found",
                                body = JsonSerializer.Serialize(new { message = "Card not found" })
                            };
                        }
                    }
                    SaveUser(User);
                    return new HttpResponse
                    {
                        status = 201,
                        message = "Deck Updated",
                        body = JsonSerializer.Serialize(new { message = "Deck Updated" })
                    };
                }




            }

                




        }








    }


    public class CardsConverter : JsonConverter<Cards>
    {
        public override Cards Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Deserialize the JSON into a JsonObject first
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;

                // Extract the properties
                var id = Guid.Parse(root.GetProperty("Id").GetString());
                var name = root.GetProperty("Name").GetString();
                var damage = root.GetProperty("Damage").GetSingle();

                // Call the constructor logic
                return new Cards(id, name, damage);
            }
        }
        public override void Write(Utf8JsonWriter writer, Cards value, JsonSerializerOptions options)
        {
            throw new NotImplementedException(); 
        }
    }

       













        }








    

