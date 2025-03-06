namespace MTCG.Backend.Server
{

    public enum ElementType
    {
        Water,
        Fire,
        Normal
    }

    public enum CardType
    {
        Monster,
        Spell
    }


    public class Cards
    {

    public Guid Id { get; set; }
    public string Name { get; set; }
    public float Damage { get; set; }
    public ElementType Element { get; set; }
    public CardType Type { get; set; }



        public Cards(Guid ID,string Name,float Damage,ElementType Element,CardType Type) { }
    
    
        public Cards(Guid id, string name, float damage)
        {

            Id = id;
            Damage = damage;

            ElementType element = ElementType.Normal;
            CardType type = name.Contains("Spell") ? CardType.Spell : CardType.Monster;

            if (name.Contains("Fire"))
            {
                element = ElementType.Fire;
                if (type == CardType.Monster)
                {
                    name = name.Replace("Fire", string.Empty).Trim();
                }
            }
            else if (name.Contains("Water"))
            {
                element = ElementType.Water;
                if (type == CardType.Monster && !name.Contains("FireElf"))
                {
                    name = name.Replace("Water", string.Empty).Trim();
                }
            }
            Name = name;
            Element = element;
            Type = type;
        }
    
    
    
    
    
    }























}