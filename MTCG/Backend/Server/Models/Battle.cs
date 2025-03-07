using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG.Backend.Server.Models
{
    public class Battle
    {
        private const int roundsLimit = 100;
        private Random rand = new Random();
        

        public class BattleResult
        {
            public string Winner { get; set; }
            public string Log { get; set; }
        }

        public BattleResult PerformBattle(User user1, User user2)
        {
            StringBuilder log = new StringBuilder();
            log.AppendLine($"Battle between {user1.Username} and {user2.Username} starts!");

            int round = 0;
            List<Cards> deck1 = new List<Cards>(user1.Cards);
            List<Cards> deck2 = new List<Cards>(user2.Cards);

            while (deck1.Count > 0 && deck2.Count > 0 && round < roundsLimit)
            {
                round++;
                log.AppendLine($"Round {round}:");

                
                Cards card1 = deck1[rand.Next(deck1.Count)];
                Cards card2 = deck2[rand.Next(deck2.Count)];

                log.AppendLine($"{user1.Username} plays {card1.Name} ({card1.Damage} dmg, {card1.Element})");
                log.AppendLine($"{user2.Username} plays {card2.Name} ({card2.Damage} dmg, {card2.Element})");

                double effective1 = CalculateEffectiveDamage(card1, card2);
                double effective2 = CalculateEffectiveDamage(card2, card1);

                // Battlecry special function
                if (effective1 < effective2)
                {
                    effective1 = Battlecry(effective1, log,card1.Name);
     
                }
                else 
                {
                    effective2 = Battlecry(effective2, log, card2.Name);
                }

                if (effective1 > effective2)
                {
                    log.AppendLine($"{user1.Username} wins the round.");
                    deck1.Add(card2);  
                    deck2.Remove(card2); 
            }
                else if (effective2 > effective1){
                    log.AppendLine($"{user2.Username} wins the round.");
                    deck2.Add(card1);  
                    deck1.Remove(card1); 
                }
                else if (effective1 == effective2)
                {
                    log.AppendLine("Round is a draw, cards remain with their owners.");
                }


                log.AppendLine(new string('-', 40));

            }

            
            string winner;
            if (deck1.Count == 0)
            {
                winner = user2.Username;
                log.AppendLine($"{user2.Username} wins the battle!");
                user2.Elo += 3;
                user1.Elo -= 5;
            }
            else if (deck2.Count == 0)
            {
                winner = user1.Username;
                log.AppendLine($"{user1.Username} wins the battle!");
                user1.Elo += 3;
                user2.Elo -= 5;
            }
            else
            {
                winner = "draw";
                log.AppendLine("The battle ended in a draw.");
            }

            Console.WriteLine(log.ToString());
            return new BattleResult { Winner = winner, Log = log.ToString() };
        }

        // Range has 1% chance to multiply the effectiveness of the weaker card
        private double Battlecry(double effectiveness, StringBuilder log, string name)
        {
        
            double lowerEffectiveness = effectiveness;

           
            double rage = rand.NextDouble();  

            if (rage < 0.01)  
            {
                lowerEffectiveness *= 5;
                log.AppendLine($"RAAAAAAAGE! Effectiveness of {name} multiplied by 5!");
            }

            return lowerEffectiveness;
        }

        public double CalculateEffectiveDamage(Cards attacker, Cards defender)
        {
            double damage = attacker.Damage;

            
            if ((attacker.Name.Contains("Goblin") && defender.Name.Contains("Dragon")) ||
            (attacker.Name.Contains("Ork") && defender.Name.Contains("Wizzard")) ||
            (attacker.Name.Contains("Knight") && defender.Type == CardType.spell && defender.Element == ElementType.Water) ||
            (attacker.Name.Contains("Dragon") && defender.Name.Contains("FireElf")))
            {
            return 0; 
            }
            if (attacker.Name.Contains("Kraken") && defender.Type == CardType.spell)
            {
            return attacker.Damage; 
            }

            
            bool hasSpell = attacker.Type == CardType.spell || defender.Type == CardType.spell;
            if (hasSpell)
            {
            if ((attacker.Element == ElementType.Water && defender.Element == ElementType.Fire) ||
                (attacker.Element == ElementType.Fire && defender.Element == ElementType.Normal) ||
                (attacker.Element == ElementType.Normal && defender.Element == ElementType.Water))
            {
                damage *= 2;  
            }
            else if ((attacker.Element == ElementType.Fire && defender.Element == ElementType.Water) ||
                 (attacker.Element == ElementType.Normal && defender.Element == ElementType.Fire) ||
                 (attacker.Element == ElementType.Water && defender.Element == ElementType.Normal))
            {
                damage /= 2; 
            }
            }

            return damage;
        }

    }
}
