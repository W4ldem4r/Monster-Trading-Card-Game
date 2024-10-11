using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MTCG.Backend.Server
{
    //strukturen des HttpResonse, User und Package KLasse
    public class HttpResponse
    {
        public int status { get; set; }
        public string message { get; set; }
        public string body { get; set; }

    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Package
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Damage { get; set; }
    }

    public class Controller
    {
        //speichern der daten in dictionaries und lists
        private readonly Dictionary<string, User> _users = new Dictionary<string, User>();
        private readonly Dictionary<string, string> _sessions = new Dictionary<string, string>();
        private readonly List<Package> _packages = new List<Package>();

        //user wird registriert und in memory abgespeichert
        public HttpResponse Register(string body)
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
        public HttpResponse Login(string body)
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

            _sessions[user.Username] = token;

              return new HttpResponse
            {
                status = 200,
                message = "OK",
                body = JsonSerializer.Serialize(new { token })
            };
        }
        //package wird erstellt und in memory abgespeichert
        public HttpResponse PackageCreation(string body)
        {
            var packages = JsonSerializer.Deserialize<List<Package>>(body);

            if (packages == null)
            {
                return new HttpResponse
                {
                    status = 400,
                    message = "Bad Request",
                    body = JsonSerializer.Serialize(new { message = "Invalid package data" })
                };
            }

            _packages.AddRange(packages);
            return new HttpResponse
            {
                status = 201,
                message = "Created",
                body = JsonSerializer.Serialize(new { message = "Packages created" })
            };
        }
    }

    
}
