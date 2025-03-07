using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using MTCG.Backend.Server.Models;


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
        public Dictionary<string, User> _users;
        public List<TradingCards> _tradingCards;
        public List<Package> _packages = new List<Package>();
        public DatabaseClass Database;
        //user wird registriert und in memory abgespeichert


        public Controller()
        {
            Database = new DatabaseClass();
            var Users = Database.LoadUsers();
            var TradingCards = Database.LoadTradingCards();

            _users = Users ?? new Dictionary<string, User>();
            _tradingCards = TradingCards ?? new List<TradingCards>();

        }





        // User Registrierung
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

            user.ID = _users.Count + 1;
            _users[user.Username] = user;
            Database.SaveUsers(_users); // User wird in DB abgespeichert

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
            else
            {
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

        // wichtige funktion. checkt den token des users
        public async Task<User?> AuthChecker(string auth)
        {
            if (_users.TryGetValue(auth, out User user))
            {
                return await Task.FromResult(user);
            }

            return null;
        }

        //userdaten werden in list und DB abgespeichert
        public void SaveUser(User ModifiedUser) 
        {
            if (_users.ContainsKey(ModifiedUser.Username))
            {
                _users[ModifiedUser.Username] = ModifiedUser;
                Database.SaveUsers(_users);
            }
            else
            {
                _users.Add(ModifiedUser.Username, ModifiedUser);
                Database.SaveUsers(_users);
            }
        }

        //ergebnisse des trades werden in list und DB gespeichert
        public void SaveTrading(TradingCards tradingCards)
        {
            _tradingCards.Add(tradingCards);
            Database.SaveTradingCards(_tradingCards);
        }

        // trading card wird aus liste entfernt und db bekommt ein update
        public void RemoveTrading(TradingCards tradingCards)
        {
            _tradingCards.Remove(tradingCards);
            Database.SaveTradingCards(_tradingCards);
        }

        // Karten des Users werden ausgegeben
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

        // deck mit 4 karten des users werden ausgegeben
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

        // plain response für die user deck anzeige (anderes format zur  besseren übersicht)
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

        // highscores werden ausgegeben
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

        // daten des users werden ausgegeben
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
            else if (userIdString != User.Username)
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

        // userdaten werden verändert und danach in liste und db gespeichert
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

        // wins und losses sowie elo werden vom user ausgegeben
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

        // package wird verkauft, geöffnet und user zugeteilt
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
            }
        }

        // karten werden zum deck des users verschoben
        public async Task<HttpResponse> PutInDeck(string body, string auth)
        {
            var User = await AuthChecker(auth);

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
                        if (card != null && !User.Deck.Contains(card))
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

        // trade wird inizialisiert
        public async Task<HttpResponse> Trading(string body, string auth)
        {
            var User = await AuthChecker(auth);
            if (User == null)
            {
                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = JsonSerializer.Serialize(new { message = "Login failed" })
                };
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return new HttpResponse
                {
                    status = 400,
                    message = "Bad Request",
                    body = JsonSerializer.Serialize(new { message = "Trading offer is empty" })
                };
            }

            TradingCards? TradingOffer;
            try
            {
                TradingOffer = JsonSerializer.Deserialize<TradingCards>(body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deserialization Error: {ex.Message}");
                return new HttpResponse
                {
                    status = 400,
                    message = "Bad Request",
                    body = JsonSerializer.Serialize(new { message = "Invalid trading offer format" })
                };
            }

            if (TradingOffer == null)
            {
                return new HttpResponse
                {
                    status = 400,
                    message = "Bad Request",
                    body = JsonSerializer.Serialize(new { message = "No Trading Offer made" })
                };
            }

            if (User.Cards.Any(card => card.Id == TradingOffer.CardToTrade))
            {
                Cards CardToTrade = User.Cards.Find(card => card.Id == TradingOffer.CardToTrade);
                TradingOffer.Card = CardToTrade;
                User.Cards.Remove(CardToTrade);
                TradingOffer.UserId = User.ID;
                SaveTrading(TradingOffer);
                SaveUser(User);
                return new HttpResponse
                {
                    status = 200,
                    message = "Trading offer Saved",
                    body = JsonSerializer.Serialize(new { message = "Trading offer Saved", TradingOffer })
                };
            }
            else if (User.Deck.Any(card => card.Id == TradingOffer.CardToTrade))
            {
                Cards CardToTrade = User.Deck.Find(card => card.Id == TradingOffer.CardToTrade);
                TradingOffer.Card = CardToTrade;
                User.Deck.Remove(CardToTrade);
                TradingOffer.UserId = User.ID;
                SaveTrading(TradingOffer);
                SaveUser(User);
                return new HttpResponse
                {
                    status = 200,
                    message = "Trading offer Saved",
                    body = JsonSerializer.Serialize(new { message = "Trading offer Saved", TradingOffer })
                };
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

        // liste an verfügbaren trade deals wird ausgegeben
        public async Task<HttpResponse> TradingList()
        {
            if (_tradingCards == null || !_tradingCards.Any()) /// Die Trading Cards liste leer?
            {
                return new HttpResponse
                {
                    status = 200,
                    message = "Trading List empty",
                    body = JsonSerializer.Serialize(new { message = _tradingCards }) /// insert list content
                };
            }
            else
            {
                return new HttpResponse
                {
                    status = 200,
                    message = "Trading Deal found",
                    body = JsonSerializer.Serialize(new { message = _tradingCards })
                };
            }
        }

        // trade wird durchgeführt, karten getauscht
        public async Task<HttpResponse> HandleTrade(string body, string auth, string path)
        {
            // Extract Trading ID from path
            string TradingIdString = path.Substring(10);

            // Try parsing CardToTrade from body
            if (!Guid.TryParse(body.Trim('"'), out Guid CardToTrade))
            {
                return new HttpResponse
                {
                    status = 400,
                    message = "Bad Request",
                    body = JsonSerializer.Serialize(new { message = "Invalid CardToTrade format." })
                };
            }

            
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

            
            if (!Guid.TryParse(TradingIdString, out Guid tradingIdGuid))
            {
                return new HttpResponse
                {
                    status = 400,
                    message = "Bad Request",
                    body = JsonSerializer.Serialize(new { message = "Invalid Trading ID" })
                };
            }

            // tradingkarte wird gefunden
            var TradingCard = _tradingCards.Find(trading => trading.Id == tradingIdGuid); //XXXX
            if (TradingCard == null)
            {
                return new HttpResponse
                {
                    status = 404,
                    message = "Not Found",
                    body = JsonSerializer.Serialize(new { message = "Trading card not found" })
                };
            }

            // User kann nicht mit sich selber handeln
            if (user.ID == TradingCard.UserId)
            {
                return new HttpResponse
                {
                    status = 400,
                    message = "Bad Request",
                    body = JsonSerializer.Serialize(new { message = "You cant trade with yourself" })
                };
            }

            // kontrolle ob user die karte in deck oder cards hat

            if (!(user.Deck.Any(card => card.Id == CardToTrade) || user.Cards.Any(card => card.Id == CardToTrade)))
            {


                return new HttpResponse
                {
                    status = 400,
                    message = "Bad Request",
                    body = JsonSerializer.Serialize(new { message = "You don't own this card." })
                };
            }

            // karte wird auf mindestanforderung kontrolliert (min damage)
            Cards? Cardcheck = user.Cards.Find(card => card.Id == CardToTrade) ?? user.Deck.Find(card => card.Id == CardToTrade);



            if (!(Cardcheck.Damage >= TradingCard.MinimumDamage))
            {
                return new HttpResponse
                {
                    status = 400,
                    message = "Bad Request",
                    body = JsonSerializer.Serialize(new { message = "Card does not meet minimum damage requirement" })
                };
            }


            User? PersonWhoSetUpTrade = _users.Values.FirstOrDefault(user => user.ID == TradingCard.UserId);

            // karten werden getauscht
            PersonWhoSetUpTrade.Cards.Add(user.Cards.Find(card => card.Id == CardToTrade));

            user.Cards.Add(TradingCard.Card);
            user.Cards.Remove(user.Cards.Find(card => card.Id == CardToTrade));
            user.Cards.Remove(user.Deck.Find(card => card.Id == CardToTrade));

            SaveUser(user);
            SaveUser(PersonWhoSetUpTrade);

            //trade anzeige wird gelöscht
            RemoveTrading(TradingCard);

            return new HttpResponse
            {
                status = 200,
                message = "OK",
                body = JsonSerializer.Serialize(new { message = "Trade successful" })
            };
        }

        public async Task<HttpResponse> DeleteTrading(string path, string auth)
        {
            string cardIdString = path.Substring(10);

            if (!Guid.TryParse(cardIdString, out Guid cardId))
            {
                return new HttpResponse
                {
                    status = 400,
                    message = "Bad Request",
                    body = JsonSerializer.Serialize(new { message = "Invalid trading card ID" })
                };
            }

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
            else ////Datenbank check nach cardid, gehört karte auch diesem user?
            {
                if (_tradingCards == null || !_tradingCards.Any()) /// Die Trading Cards liste leer?
                {
                    return new HttpResponse
                    {
                        status = 200,
                        message = "Trading List empty",
                        body = JsonSerializer.Serialize(new { message = _tradingCards })
                    };
                }
                else
                {
                    
                    var delCard = _tradingCards.FirstOrDefault(card => card.Id == cardId);

                    if (delCard == null)
                    {
                        return new HttpResponse
                        {
                            status = 404,
                            message = "Not Found",
                            body = JsonSerializer.Serialize(new { message = "Trading deal not found" })
                        };
                    }
                    else
                    {

                        if (delCard.UserId != User.ID)
                        {
                            return new HttpResponse
                            {
                                status = 404,
                                message = "Not Your Offer",
                                body = JsonSerializer.Serialize(new { message = "Not Your Offer" })
                            };
                        }

                        else
                        {
                            User.Cards.Add(delCard.Card);
                            SaveUser(User);
                            RemoveTrading(delCard);


                            return new HttpResponse
                            {
                                status = 200,
                                message = "OK",
                                body = JsonSerializer.Serialize(new { message = "Trading deal deleted" })
                            };

                        }

                    }
                }
            }
        }



        private static ConcurrentQueue<User> battleLobby = new ConcurrentQueue<User>();
        private static readonly object battleLock = new object();

        // battle handler
        public async Task<HttpResponse> Battle(string auth)
        {
            User? user = await AuthChecker(auth);
            if (user == null)
            {
                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = JsonSerializer.Serialize(new { message = "Login required to join battle" })
                };
            }

            // player wird in lobby gebracht
            battleLobby.Enqueue(user);
            Console.WriteLine($"{user.Username} has joined the battle lobby.");

            User player1, player2;

            // lock 
            lock (battleLock)
            {
                if (battleLobby.Count >= 2 &&
                    battleLobby.TryDequeue(out player1) &&
                    battleLobby.TryDequeue(out player2))
                {
                    // Start the battle in a separate task
                    _ = Task.Run(() => StartBattle(player1, player2));

                    return new HttpResponse
                    {
                        status = 200,
                        message = "Battle started",
                        body = JsonSerializer.Serialize(new { message = $"{player1.Username} vs {player2.Username}" })
                    };
                }
            }

            return new HttpResponse
            {
                status = 200,
                message = "Waiting for opponent...",
                body = JsonSerializer.Serialize(new { message = "Waiting for another player to join..." })
            };
        }

        private async Task StartBattle(User player1, User player2)
        {
            Console.WriteLine($"Battle started between {player1.Username} and {player2.Username}");

            Battle battle = new Battle();
            Battle.BattleResult result = battle.PerformBattle(player1, player2);

            Console.WriteLine(result.Log);


            // Determine winner and update ELO
            if (result.Winner == player1.Username)
            {
                player1.Wins++;
                player2.Losses++;
            }
            else if (result.Winner == player2.Username)
            {
                player2.Wins++;
                player1.Losses++;
            }

            // Save updated stats (assuming a SaveUser function exists)
            SaveUser(player1);
            SaveUser(player2);
        }


        public async Task<HttpResponse> UpgradeRandomCard(string auth)
        {
            var User = await AuthChecker(auth);
            if (User == null)
            {
                return new HttpResponse
                {
                    status = 401,
                    message = "Unauthorized",
                    body = JsonSerializer.Serialize(new { message = "Login failed" })
                };
            }
            if (User.Cards.Count == 0 )
            {
                return new HttpResponse
                {
                    status = 404,
                    message = "Not Found",
                    body = JsonSerializer.Serialize(new { message = "No cards to upgrade" })
                };
            }
            if (User.Coins < 5)
            {
                return new HttpResponse
                {
                    status = 404,
                    message = "Not Found",
                    body = JsonSerializer.Serialize(new { message = "Not enough coins" })
                };
            }
            else
            {

                User.Coins -= 5;
                Random random = new Random();
                int index = random.Next(User.Cards.Count);
                float damageIncrease = (float)random.Next(5, 11);
                User.Cards[index].Damage += damageIncrease;
                SaveUser(User);
                return new HttpResponse
                {
                    status = 200,
                    message = "OK",
                    body = JsonSerializer.Serialize(new { message = $"Card upgraded {User.Cards[index].Name}" })
                };
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