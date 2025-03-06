
namespace MTCG.Backend.Server
{

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int ID { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public int Elo { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Coins { get; set; }
        public List<Cards> Cards{ get; set; }
        public List<Cards> Deck { get; set; }

        public User(int id, string username, string password) // User wird Regist
        {
            ID = id;
            Username = username;
            Password = password;
            Bio = string.Empty;
            Image = string.Empty;
            Name = string.Empty;
            Elo = 100;
            Wins = 0;
            Losses = 0;
            Coins = 20;
            Cards = new List<Cards>();
            Deck = new List<Cards>();
        }


    }

}