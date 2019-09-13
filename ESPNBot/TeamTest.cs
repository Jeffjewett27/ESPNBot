using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace ESPNBot
{
    [TestFixture]
    class TeamTest
    {
        Player[] players =
            {
                new Player() { name = "Abby", position = Position.Quarterback, eligibility = Eligibility.AOK, team = "SEA" },
                new Player() { name = "Bob", position = Position.RunningBack, eligibility = Eligibility.AOK, team = "SEA" },
                new Player() { name = "Charlie", position = Position.RunningBack, eligibility = Eligibility.AOK, team = "ATL" },
                new Player() { name = "Dave", position = Position.WideReceiver, eligibility = Eligibility.AOK, team = "MIN" },
                new Player() { name = "Ernest", position = Position.WideReceiver, eligibility = Eligibility.AOK, team = "LAR" },
                new Player() { name = "Fred", position = Position.TightEnd, eligibility = Eligibility.AOK, team = "NYJ" },
                new Player() { name = "George", position = Position.TightEnd, eligibility = Eligibility.AOK, team = "GBP" },
                new Player() { name = "Hilbert", position = Position.Defense, eligibility = Eligibility.AOK, team = "OAK" },
                new Player() { name = "Ian", position = Position.Kicker, eligibility = Eligibility.AOK, team = "LAC" },
                new Player() { name = "Jeff", position = Position.TightEnd, eligibility = Eligibility.Injured, team = "SEA" },
                new Player() { name = "Kyle", position = Position.Quarterback, eligibility = Eligibility.AOK, team = "MIN" },
                new Player() { name = "Luke", position = Position.WideReceiver, eligibility = Eligibility.AOK, team = "PHI" },
                new Player() { name = "Manny", position = Position.Kicker, eligibility = Eligibility.Out, team = "BUF" },
                new Player() { name = "Noah", position = Position.Defense, eligibility = Eligibility.AOK, team = "CHI" },
                new Player() { name = "Owen", position = Position.WideReceiver, eligibility = Eligibility.AOK, team = "HOU" },
                new Player() { name = "Phillip", position = Position.Quarterback, eligibility = Eligibility.Questionable, team = "WSH" }
            };

        [Test]
        public void TestRoster()
        {
            var roster = new Roster(players);
        }

        [Test]
        public void TestInvalidRoster()
        {
            Player temp = players[4];
            players[4] = players[8];
            players[8] = temp;

            try
            {
                var roster = new Roster(players);
            } catch
            {
                Assert.Pass();
            }
            Assert.Fail();
        }

        [Test]
        public void TestInsufficientRoster()
        {
            Player[] insufficient = new Player[10];
            Array.Copy(players, 6, insufficient, 0, 10);

            try
            {
                var roster = new Roster(insufficient);
            } catch
            {
                Assert.Pass();
            }
            Assert.Fail();
        }

        [Test]
        public void TestRosterSwap()
        {
            var roster = new Roster(players);

            Player firstQB = roster.starters[0];
            Player newGuy = roster.SubOut(0, 2); //sub out starter 0

            Assert.AreNotEqual(firstQB, newGuy);

            Assert.AreNotEqual(firstQB, roster.starters[0]);

            Assert.AreEqual(newGuy, roster.starters[0]);
        }
    }
}
