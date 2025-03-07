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
        monster,
        spell
    }

    public class Cards
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public float Damage { get; set; }
        public ElementType Element { get; set; }
        public CardType Type { get; set; }

        public Cards(Guid ID, string CardName, float CardDamage, ElementType CardElement, CardType CardTypes) {

            Id = ID;
            Name = CardName;
            Damage = CardDamage;
            Element = CardElement;
            Type = CardTypes;
        }

        public Cards(Guid id, string name, float damage)
        {
            Id = id;
            Damage = damage;

            ElementType element = ElementType.Normal;
            CardType type = name.Contains("Spell") ? CardType.spell : CardType.monster;

            if (name.Contains("Fire"))
            {
                element = ElementType.Fire;
                if (type == CardType.monster && !name.Contains("FireElf"))
                {
                    name = name.Replace("Fire", string.Empty).Trim();
                }
            }
            else if (name.Contains("Water"))
            {
                element = ElementType.Water;
                if (type == CardType.monster)
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