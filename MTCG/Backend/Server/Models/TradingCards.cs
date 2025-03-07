using MTCG.Backend.Server;
using System.Text.Json;
using System.Text.Json.Serialization;

public class TradingCards
{
    public Guid Id { get; set; }
    public Guid CardToTrade { get; set; }
    public string Type { get; set; }
    public int MinimumDamage { get; set; }
    public int UserId { get; set; }
    public Cards Card { get; set; }

    public TradingCards(Guid id, Guid cardToTrade, string type, int mindamage, int userId)
    {
        Id = id;
        CardToTrade = cardToTrade;
        Type = type;
        MinimumDamage = mindamage;
        UserId = userId;
    }
    public TradingCards(Guid id, Guid cardToTrade, string type, int mindamage, int userId,Cards TheCard)
    {
        Id = id;
        CardToTrade = cardToTrade;
        Type = type;
        MinimumDamage = mindamage;
        UserId = userId;
        Card = TheCard;
    }





    public TradingCards() { }
}