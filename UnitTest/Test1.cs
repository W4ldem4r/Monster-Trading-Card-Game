using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTCG.Backend.Server;
using MTCG.Backend.Server.Models;
using MTCG.Backend;
using System;
using System.Collections.Generic;


namespace UnitTest
{
    [TestClass]
    public class UserTests
    {
        [TestMethod]
        public void CheckingUserAtCreation()
        {
            var user = new User(1, "testuser", "password123");
            Assert.AreEqual(1, user.ID);
            Assert.AreEqual("testuser", user.Username);
            Assert.AreEqual("password123", user.Password);
            Assert.AreEqual(string.Empty, user.Bio);
            Assert.AreEqual(string.Empty, user.Image);
            Assert.AreEqual(string.Empty, user.Name);
            Assert.AreEqual(100, user.Elo);
            Assert.AreEqual(0, user.Wins);
            Assert.AreEqual(0, user.Losses);
            Assert.AreEqual(20, user.Coins);
            Assert.IsNotNull(user.Cards);
            Assert.IsNotNull(user.Deck);
            Assert.AreEqual(0, user.Cards.Count);
            Assert.AreEqual(0, user.Deck.Count);
        }

        [TestMethod]
        public void UpdatingUserBio()
        {
            var user = new User(1, "testuser", "password123");
            user.Bio = "This is a test bio.";
            Assert.AreEqual("This is a test bio.", user.Bio);
        }


        [TestMethod]
        public void TestUserEloUpdate()
        {
            var user = new User(1, "testuser", "password123");
            var user2 = new User(2, "testuser2", "password123");
            user.Elo = 120;
            user2.Elo = 100;

            Assert.AreEqual(120, user.Elo);
        }

        [TestMethod]
        public void TestUserCoinsUpdate()
        {
            var user = new User(1, "testuser", "password123");
            Assert.AreEqual(20, user.Coins);
        }

        [TestMethod]
        public void UserCardAdd()
        {
            var user = new User(1, "testuser", "password123");
            var card = new Cards(Guid.NewGuid(), "Fireball", 50.0f, ElementType.Fire, CardType.spell);
            user.Cards.Add(card);
            Assert.AreEqual(1, user.Cards.Count);
            Assert.AreEqual("Fireball", user.Cards[0].Name);
        }

        [TestMethod]
        public void UserDeckAdd()
        {
            var user = new User(1, "testuser", "password123");
            var card = new Cards(Guid.NewGuid(), "Fireball", 50.0f, ElementType.Fire, CardType.spell);
            user.Deck.Add(card);
            Assert.AreEqual(1, user.Deck.Count);
            Assert.AreEqual("Fireball", user.Deck[0].Name);
        }
    }
    [TestClass]
    public class CardsTests
    {
        [TestMethod]
        public void CardCreationWithAll()
        {
            var card = new Cards(Guid.NewGuid(), "Fireball", 50.0f, ElementType.Fire, CardType.spell);
            Assert.AreEqual("Fireball", card.Name);
            Assert.AreEqual(50.0f, card.Damage);
            Assert.AreEqual(ElementType.Fire, card.Element);
            Assert.AreEqual(CardType.spell, card.Type);
        }

        [TestMethod]
        public void CardCreationWith2Const()
        {
            var card = new Cards(Guid.NewGuid(), "WaterGoblin", 30.0f);
            Assert.AreEqual("Goblin", card.Name);
            Assert.AreEqual(30.0f, card.Damage);
            Assert.AreEqual(ElementType.Water, card.Element);
            Assert.AreEqual(CardType.monster, card.Type);
        }

        [TestMethod]
        public void CardCreationNormal()
        {
            var card = new Cards(Guid.NewGuid(), "NormalSpell", 20.0f);
            Assert.AreEqual("NormalSpell", card.Name);
            Assert.AreEqual(20.0f, card.Damage);
            Assert.AreEqual(ElementType.Normal, card.Element);
            Assert.AreEqual(CardType.spell, card.Type);
        }

        [TestMethod]
        public void CardCreationFire()
        {
            var card = new Cards(Guid.NewGuid(), "FireSpell", 40.0f);
            Assert.AreEqual("FireSpell", card.Name);
            Assert.AreEqual(40.0f, card.Damage);
            Assert.AreEqual(ElementType.Fire, card.Element);
            Assert.AreEqual(CardType.spell, card.Type);
        }

        [TestMethod]
        public void CardCreationFireElf()
        {
            var card = new Cards(Guid.NewGuid(), "FireElf", 40.0f);
            Assert.AreEqual("FireElf", card.Name);
            Assert.AreEqual(40.0f, card.Damage);
            Assert.AreEqual(ElementType.Fire, card.Element);
            Assert.AreEqual(CardType.monster, card.Type);
        }

        [TestMethod]
        public void CardCreationWater()
        {
            var card = new Cards(Guid.NewGuid(), "WaterSpell", 25.0f);
            Assert.AreEqual("WaterSpell", card.Name);
            Assert.AreEqual(25.0f, card.Damage);
            Assert.AreEqual(ElementType.Water, card.Element);
            Assert.AreEqual(CardType.spell, card.Type);
        }
    }

    [TestClass]
    public class PackageTests
    {
        [TestMethod]
        public void PackageCreation()
        {
            var card1 = new Cards(Guid.NewGuid(), "Fireball", 50.0f, ElementType.Fire, CardType.spell);
            var card2 = new Cards(Guid.NewGuid(), "WaterGoblin", 30.0f, ElementType.Water, CardType.monster);
            var cards = new List<Cards> { card1, card2 };
            var package = new Package(cards);

            Assert.IsNotNull(package.allCards);
            Assert.AreEqual(2, package.allCards.Count);
            Assert.AreEqual("Fireball", package.allCards[0].Name);
            Assert.AreEqual("WaterGoblin", package.allCards[1].Name);
        }

        [TestMethod]
        public void PackageWithNoCards()
        {
            var cards = new List<Cards>();
            var package = new Package(cards);

            Assert.IsNotNull(package.allCards);
            Assert.AreEqual(0, package.allCards.Count);
        }

        [TestMethod]
        public void PackageWithNullCardList()
        {
            List<Cards> cards = null;
            var package = new Package(cards);

            Assert.IsNull(package.allCards);
        }
        [TestMethod]
        public void UserOpeningPacket()
        {   
            
            
            var user = new User(1, "testuser", "password123");
           
            
            var card1 = new Cards(Guid.NewGuid(), "Fireball", 50.0f, ElementType.Fire, CardType.spell);
            var card2 = new Cards(Guid.NewGuid(), "WaterGoblin", 30.0f, ElementType.Water, CardType.monster);
            var cards = new List<Cards> { card1, card2 };
            var package = new Package(cards);

            user.Cards.AddRange(package.allCards);

            Assert.AreEqual(2, user.Cards.Count);
            Assert.AreEqual("Fireball", user.Cards[0].Name);
            Assert.AreEqual("WaterGoblin", user.Cards[1].Name);
        }
    }
    [TestClass]
    public class BattleTests
    {
        [TestMethod]
        public void BattleTwoUsers()
        {
            var user1 = new User(1, "user1", "password1");
            var user2 = new User(2, "user2", "password2");

            user1.Cards.Add(new Cards(Guid.NewGuid(), "Fireball", 50.0f, ElementType.Fire, CardType.spell));
            user2.Cards.Add(new Cards(Guid.NewGuid(), "WaterGoblin", 30.0f, ElementType.Water, CardType.monster));

            var battle = new Battle();
            var result = battle.PerformBattle(user1, user2);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Winner == "user1" || result.Winner == "user2" || result.Winner == "draw");
            Assert.IsNotNull(result.Log);
        }

        [TestMethod]
        public void CalculateeDamage()
        {
            var battle = new Battle();

            var card1 = new Cards(Guid.NewGuid(), "Fireball", 50.0f, ElementType.Fire, CardType.spell);
            var card2 = new Cards(Guid.NewGuid(), "WaterGoblin", 30.0f, ElementType.Water, CardType.monster);

            double damage = battle.CalculateEffectiveDamage(card1, card2);

            Assert.AreEqual(25.0, damage);
        }

        [TestMethod]
        public void CalculateDamageAdvantage()
        {
            var battle = new Battle();

            var card1 = new Cards(Guid.NewGuid(), "WaterSpell", 40.0f, ElementType.Water, CardType.spell);
            var card2 = new Cards(Guid.NewGuid(), "FireGoblin", 30.0f, ElementType.Fire, CardType.monster);

            double damage = battle.CalculateEffectiveDamage(card1, card2);

            Assert.AreEqual(80.0, damage);
        }

        [TestMethod]
        public void CalculateDamageNeutral()
        {
            var battle = new Battle();

            var card1 = new Cards(Guid.NewGuid(), "NormalSpell", 40.0f, ElementType.Normal, CardType.spell);
            var card2 = new Cards(Guid.NewGuid(), "NormalGoblin", 30.0f, ElementType.Normal, CardType.monster);

            double damage = battle.CalculateEffectiveDamage(card1, card2);

            Assert.AreEqual(40.0, damage); 
        }

 
    }
}
