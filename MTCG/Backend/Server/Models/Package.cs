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


}