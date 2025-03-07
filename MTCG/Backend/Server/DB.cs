using Npgsql;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MTCG.Backend.Server
{

    public class DatabaseClass
    {
        private readonly string dbconnection = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";

         private string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return Regex.Replace(input, "[;'\"\\\\]", "");
        }


        public DatabaseClass()
        {
            StartCheck();

            Console.WriteLine("Do you want to delete all tables? (y/n)");
             string question = Console.ReadLine();

           if (question?.ToLower() == "y")
            {
                using (var connection = new NpgsqlConnection(dbconnection))
                {
                    connection.Open();
                    DropTables(connection);
                }
                using (var connection = new NpgsqlConnection(dbconnection))
                {
                    connection.Open();
                    InitDatabase(connection);
                }
            }

        }

        public void StartCheck()
        {
            try
            {
                using (var connection = new NpgsqlConnection(dbconnection))
                {
                    connection.Open();
                    Console.WriteLine("Database connection successful.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Database is not online - {ex.Message}");
                throw;
            }
        }

        public void DropTables(NpgsqlConnection connection)
        {
            string dropTables = @"
        DROP TABLE IF EXISTS UserCards CASCADE;
        DROP TABLE IF EXISTS Decks CASCADE;
        DROP TABLE IF EXISTS TradingCards CASCADE;
        DROP TABLE IF EXISTS Cards CASCADE;
        DROP TABLE IF EXISTS Users CASCADE;
    ";

            try
            {
                using (var cmd = new NpgsqlCommand(dropTables, connection))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("All tables dropped successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error dropping tables: {ex.Message}");
                throw;
            }
        }

        public void InitDatabase(NpgsqlConnection connection)
        {
            string createTables = @"
        CREATE TABLE IF NOT EXISTS Users (
            ID SERIAL PRIMARY KEY,
            Username VARCHAR(50) UNIQUE NOT NULL,
            Password TEXT NOT NULL,
            Bio TEXT,
            Image TEXT,
            Name TEXT,
            Elo INTEGER,
            Wins INTEGER,
            Losses INTEGER,
            Coins INTEGER
        );

        CREATE TABLE IF NOT EXISTS Cards (
            ID UUID PRIMARY KEY,
            Name VARCHAR(100) NOT NULL,
            Damage FLOAT NOT NULL,
            ElementType VARCHAR(10) CHECK (ElementType IN ('Water', 'Fire', 'Normal')),
            CardType VARCHAR(10) CHECK (CardType IN ('monster', 'spell'))
        );

        CREATE TABLE IF NOT EXISTS UserCards (
            UserID INT REFERENCES Users(ID) ON DELETE CASCADE,
            CardID UUID REFERENCES Cards(ID) ON DELETE CASCADE,
            PRIMARY KEY (UserID, CardID)
        );

        CREATE TABLE IF NOT EXISTS Decks (
            UserID INT REFERENCES Users(ID) ON DELETE CASCADE,
            CardID UUID REFERENCES Cards(ID) ON DELETE CASCADE,
            PRIMARY KEY (UserID, CardID)
        );

        CREATE TABLE IF NOT EXISTS TradingCards (
            ID UUID PRIMARY KEY,
            CardToTrade UUID REFERENCES Cards(ID) ON DELETE CASCADE,
            Type VARCHAR(20) NOT NULL,
            MinimumDamage INT NOT NULL,
            UserID INT REFERENCES Users(ID) ON DELETE CASCADE
        );
    ";

            try
            {
                using (var cmd = new NpgsqlCommand(createTables, connection))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Database tables created successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating tables: {ex.Message}");
                throw;
            }
        }


        public void SaveUsers(Dictionary<string, User> _users)
        {
            using (var connection = new NpgsqlConnection(dbconnection))
            {
                connection.Open();
                foreach (var user in _users.Values)
                {
                    string insertUser = @"
                INSERT INTO Users (ID, Username, Password, Bio, Image, Name, Elo, Wins, Losses, Coins)
                VALUES (@ID, @Username, @Password, @Bio, @Image, @Name, @Elo, @Wins, @Losses, @Coins)
                ON CONFLICT (Username) DO UPDATE
                SET 
                    Password = EXCLUDED.Password,
                    Bio = EXCLUDED.Bio,
                    Image = EXCLUDED.Image,
                    Name = EXCLUDED.Name,
                    Elo = EXCLUDED.Elo,
                    Wins = EXCLUDED.Wins,
                    Losses = EXCLUDED.Losses,
                    Coins = EXCLUDED.Coins;
            ";

                    try
                    {
                        using (var cmd = new NpgsqlCommand(insertUser, connection))
                        {
                            cmd.Parameters.AddWithValue("ID", user.ID);
                            cmd.Parameters.AddWithValue("Username", SanitizeInput(user.Username));
                            cmd.Parameters.AddWithValue("Password", SanitizeInput(user.Password));
                            cmd.Parameters.AddWithValue("Bio", SanitizeInput(user.Bio));
                            cmd.Parameters.AddWithValue("Image", SanitizeInput(user.Image));
                            cmd.Parameters.AddWithValue("Name", SanitizeInput(user.Name));
                            cmd.Parameters.AddWithValue("Elo", user.Elo);
                            cmd.Parameters.AddWithValue("Wins", user.Wins);
                            cmd.Parameters.AddWithValue("Losses", user.Losses);
                            cmd.Parameters.AddWithValue("Coins", user.Coins);
                            cmd.ExecuteNonQuery();
                        }

                        SaveUserCards(user, connection);
                        SaveUserDeck(user, connection);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving user {user.Username}: {ex.Message}");
                        throw;
                    }
                }
                Console.WriteLine("Users saved successfully.");
            }
        }

        private void SaveUserCards(User user, NpgsqlConnection connection)
        {
            foreach (var card in user.Cards)
            {
                string insertCard = @"
            INSERT INTO Cards (ID, Name, Damage, ElementType, CardType)
            VALUES (@ID, @Name, @Damage, @ElementType, @CardType)
            ON CONFLICT (ID) DO UPDATE
            SET 
                Name = EXCLUDED.Name,
                Damage = EXCLUDED.Damage,
                ElementType = EXCLUDED.ElementType,
                CardType = EXCLUDED.CardType;
        ";

                try
                {
                    using (var cmd = new NpgsqlCommand(insertCard, connection))
                    {
                        cmd.Parameters.AddWithValue("ID", card.Id);
                        cmd.Parameters.AddWithValue("Name", card.Name);
                        cmd.Parameters.AddWithValue("Damage", card.Damage);
                        cmd.Parameters.AddWithValue("ElementType", card.Element.ToString());
                        cmd.Parameters.AddWithValue("CardType", card.Type.ToString());
                        cmd.ExecuteNonQuery();
                    }

                    string insertUserCard = @"
                INSERT INTO UserCards (UserID, CardID)
                VALUES (@UserID, @CardID)
                ON CONFLICT (UserID, CardID) DO UPDATE
                SET CardID = EXCLUDED.CardID;
            ";

                    using (var cmd = new NpgsqlCommand(insertUserCard, connection))
                    {
                        cmd.Parameters.AddWithValue("UserID", user.ID);
                        cmd.Parameters.AddWithValue("CardID", card.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving card {card.Name} for user {user.Username}: {ex.Message}");
                    throw;
                }
            }
        }

        private void SaveUserDeck(User user, NpgsqlConnection connection)
        {
            foreach (var card in user.Deck)
            {
                string insertDeckCard = @"
            INSERT INTO Decks (UserID, CardID)
            VALUES (@UserID, @CardID)
            ON CONFLICT (UserID, CardID) DO UPDATE
            SET CardID = EXCLUDED.CardID;
        ";

                try
                {
                    using (var cmd = new NpgsqlCommand(insertDeckCard, connection))
                    {
                        cmd.Parameters.AddWithValue("UserID", user.ID);
                        cmd.Parameters.AddWithValue("CardID", card.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving deck card {card.Name} for user {user.Username}: {ex.Message}");
                    throw;
                }
            }
        }

        public void SaveTradingCards(List<TradingCards> _tradingCards)
        {
            using (var connection = new NpgsqlConnection(dbconnection))
            {
                connection.Open();
                foreach (var tradingCard in _tradingCards)
                {
                    string insertTradingCard = @"
                INSERT INTO TradingCards (ID, CardToTrade, Type, MinimumDamage, UserID)
                VALUES (@ID, @CardToTrade, @Type, @MinimumDamage, @UserID)
                ON CONFLICT (ID) DO UPDATE
                SET 
                    CardToTrade = EXCLUDED.CardToTrade,
                    Type = EXCLUDED.Type,
                    MinimumDamage = EXCLUDED.MinimumDamage,
                    UserID = EXCLUDED.UserID;
            ";

                    try
                    {
                        using (var cmd = new NpgsqlCommand(insertTradingCard, connection))
                        {
                            cmd.Parameters.AddWithValue("ID", tradingCard.Id);
                            cmd.Parameters.AddWithValue("CardToTrade", tradingCard.CardToTrade);
                            cmd.Parameters.AddWithValue("Type", tradingCard.Type);
                            cmd.Parameters.AddWithValue("MinimumDamage", tradingCard.MinimumDamage);
                            cmd.Parameters.AddWithValue("UserID", tradingCard.UserId);
                            cmd.ExecuteNonQuery();
                        }

                        SaveCard(tradingCard.Card, connection);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving trading card {tradingCard.Id}: {ex.Message}");
                        throw;
                    }
                }
                Console.WriteLine("Trading cards saved successfully.");
            }
        }

        private void SaveCard(Cards card, NpgsqlConnection connection)
        {
            string insertCard = @"
        INSERT INTO Cards (ID, Name, Damage, ElementType, CardType)
        VALUES (@ID, @Name, @Damage, @ElementType, @CardType)
        ON CONFLICT (ID) DO UPDATE
        SET 
            Name = EXCLUDED.Name,
            Damage = EXCLUDED.Damage,
            ElementType = EXCLUDED.ElementType,
            CardType = EXCLUDED.CardType;
    ";

            try
            {
                using (var cmd = new NpgsqlCommand(insertCard, connection))
                {
                    cmd.Parameters.AddWithValue("ID", card.Id);
                    cmd.Parameters.AddWithValue("Name", card.Name);
                    cmd.Parameters.AddWithValue("Damage", card.Damage);
                    cmd.Parameters.AddWithValue("ElementType", card.Element.ToString());
                    cmd.Parameters.AddWithValue("CardType", card.Type.ToString());
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving card {card.Name}: {ex.Message}");
                throw;
            }
        }

        public Dictionary<string, User> LoadUsers()
        {
            var users = new Dictionary<string, User>();

            using (var connection = new NpgsqlConnection(dbconnection))
            {
                connection.Open();
                string query = "SELECT * FROM Users";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new User(
                            reader.GetInt32(reader.GetOrdinal("ID")),
                            reader.GetString(reader.GetOrdinal("Username")),
                            reader.GetString(reader.GetOrdinal("Password"))
                        )
                        {
                            Bio = reader.GetString(reader.GetOrdinal("Bio")),
                            Image = reader.GetString(reader.GetOrdinal("Image")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Elo = reader.GetInt32(reader.GetOrdinal("Elo")),
                            Wins = reader.GetInt32(reader.GetOrdinal("Wins")),
                            Losses = reader.GetInt32(reader.GetOrdinal("Losses")),
                            Coins = reader.GetInt32(reader.GetOrdinal("Coins"))
                        };

                        users.Add(user.Username, user);
                    }
                }

                // Load user cards and decks after closing the reader
                foreach (var user in users.Values)
                {
                    user.Cards = LoadUserCards(user.ID, connection);
                    user.Deck = LoadUserDeck(user.ID, connection);
                }
            }

            return users;
        }

        private List<Cards> LoadUserCards(int userId, NpgsqlConnection connection)
        {
            var cards = new List<Cards>();
            string query = @"
        SELECT c.* FROM Cards c
        JOIN UserCards uc ON c.ID = uc.CardID
        WHERE uc.UserID = @UserID
    ";

            using (var cmd = new NpgsqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("UserID", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var card = new Cards(
                            reader.GetGuid(reader.GetOrdinal("ID")),
                            reader.GetString(reader.GetOrdinal("Name")),
                            reader.GetFloat(reader.GetOrdinal("Damage")),
                            (ElementType)Enum.Parse(typeof(ElementType), reader.GetString(reader.GetOrdinal("ElementType"))),
                            (CardType)Enum.Parse(typeof(CardType), reader.GetString(reader.GetOrdinal("CardType")))
                        );

                        cards.Add(card);
                    }
                }
            }

            return cards;
        }

        private List<Cards> LoadUserDeck(int userId, NpgsqlConnection connection)
        {
            var deck = new List<Cards>();
            string query = @"
        SELECT c.* FROM Cards c
        JOIN Decks d ON c.ID = d.CardID
        WHERE d.UserID = @UserID
    ";

            using (var cmd = new NpgsqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("UserID", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var card = new Cards(
                            reader.GetGuid(reader.GetOrdinal("ID")),
                            reader.GetString(reader.GetOrdinal("Name")),
                            reader.GetFloat(reader.GetOrdinal("Damage")),
                            (ElementType)Enum.Parse(typeof(ElementType), reader.GetString(reader.GetOrdinal("ElementType"))),
                            (CardType)Enum.Parse(typeof(CardType), reader.GetString(reader.GetOrdinal("CardType")))
                        );

                        deck.Add(card);
                    }
                }
            }

            return deck;
        }




        public List<TradingCards> LoadTradingCards()
        {
            var tradingCards = new List<TradingCards>();

            try
            {
                using (var connection = new NpgsqlConnection(dbconnection))
                {
                    connection.Open();
                    string query = @"
                SELECT tc.*, c.ID as CardID, c.Name, c.Damage, c.ElementType, c.CardType 
                FROM TradingCards tc
                JOIN Cards c ON tc.CardToTrade = c.ID";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {


                            Guid Id = reader.GetGuid(reader.GetOrdinal("ID"));
                            Guid CardToTrade = reader.GetGuid(reader.GetOrdinal("CardToTrade"));
                            string Type = reader.GetString(reader.GetOrdinal("Type"));
                            int MinimumDamage = reader.GetInt32(reader.GetOrdinal("MinimumDamage"));
                            int UserId = reader.GetInt32(reader.GetOrdinal("UserID"));


                            Guid CardID = reader.GetGuid(reader.GetOrdinal("CardID"));
                            Console.WriteLine(CardID);
                            string Name = reader.GetString(reader.GetOrdinal("Name"));
                            Console.WriteLine(Name);
                            float Damage = reader.GetFloat(reader.GetOrdinal("Damage"));
                            Console.WriteLine(Damage);
                            ElementType CardEType = (ElementType)Enum.Parse(typeof(ElementType), reader.GetString(reader.GetOrdinal("ElementType")));
                            Console.WriteLine(CardEType);
                            CardType cardType = (CardType)Enum.Parse(typeof(CardType), reader.GetString(reader.GetOrdinal("Type")));
                            Console.WriteLine(cardType);

                            Cards Card = new Cards(CardID, Name, Damage, CardEType, cardType);

                            Console.WriteLine(Card.Name);
                            var tradingCard = new TradingCards(Id, CardToTrade, Type, MinimumDamage, UserId, Card);
             

                            tradingCards.Add(tradingCard);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading trading cards: {ex.Message}");
                throw;
            }

            return tradingCards;
        }

    }
}
