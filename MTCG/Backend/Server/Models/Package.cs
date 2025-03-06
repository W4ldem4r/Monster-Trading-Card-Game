namespace MTCG.Backend.Server
{


    public class Package
    {
        public List<Cards> allCards { get; set; }

        public Package(List<Cards> CardsInside)
        {
            allCards = CardsInside;
        }
    }



    /*


    public class PackageToCard
    {
        public List<Package> PackageCards { get; set; }

        public PackageToCard(List<Package> packageCardsGiven)
        {
            PackageCards = packageCardsGiven ?? new List<Package>();
        }

        public List<Cards> GetAllCards()
        {
            List<Cards> allCards = new List<Cards>();

            foreach (var package in PackageCards)
            {
                string name = package.Name;
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

                allCards.Add(new Cards(package.Id, name, package.Damage, element, type));
            }

            return allCards;
        }
    }

    
    */







}