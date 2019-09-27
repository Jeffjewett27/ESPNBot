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
        Player[] players;
        Player[] freeAgents;

        private void SetPlayers()
        {
            players = new Player[16]
            {
                new Player() { name = "Abby", position = Position.Quarterback, eligibility = Eligibility.AOK, team = "SEA" },
                new Player() { name = "Bob", position = Position.RunningBack, eligibility = Eligibility.AOK, team = "SEA" },
                new Player() { name = "Charlie", position = Position.RunningBack, eligibility = Eligibility.AOK, team = "ATL" },
                new Player() { name = "Dave", position = Position.WideReceiver, eligibility = Eligibility.AOK, team = "MIN" },
                new Player() { name = "Ernest", position = Position.WideReceiver, eligibility = Eligibility.AOK, team = "LAR" },
                new Player() { name = "Fred", position = Position.TightEnd, eligibility = Eligibility.AOK, team = "NYJ" },
                new Player() { name = "George", position = Position.TightEnd, eligibility = Eligibility.AOK, team = "GBP" }, //flex
                new Player() { name = "Hilbert", position = Position.Defense, eligibility = Eligibility.AOK, team = "OAK" },
                new Player() { name = "Ian", position = Position.Kicker, eligibility = Eligibility.AOK, team = "LAC" },
                new Player() { name = "Jeff", position = Position.TightEnd, eligibility = Eligibility.Injured, team = "SEA" },
                new Player() { name = "Kyle", position = Position.Quarterback, eligibility = Eligibility.AOK, team = "MIN" },
                new Player() { name = "Luke", position = Position.WideReceiver, eligibility = Eligibility.AOK, team = "PHI" },
                new Player() { name = "Manny", position = Position.WideReceiver, eligibility = Eligibility.Out, team = "BUF" },
                new Player() { name = "Noah", position = Position.Defense, eligibility = Eligibility.AOK, team = "CHI" },
                new Player() { name = "Owen", position = Position.WideReceiver, eligibility = Eligibility.AOK, team = "HOU" },
                new Player() { name = "Phillip", position = Position.Quarterback, eligibility = Eligibility.Questionable, team = "WSH" }
            };
        }

        private void SetFreeAgents()
        {
            freeAgents = new Player[1] {
                new Player() { name = "Quinn", position = Position.RunningBack, eligibility = Eligibility.AOK, team = "SEA" }
            };
        }

        [Test]
        public void TestRoster()
        {
            SetPlayers();
            var roster = new Roster(players);
        }

        [Test]
        public void TestInvalidRoster()
        {
            SetPlayers();
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
            SetPlayers();
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

        [TestCase(0, true)]
        [TestCase(8, false)]
        [TestCase(5, true)]
        [TestCase(1, false)]
        [TestCase(6, false)]
        public void TestRosterSub(int startID, bool success)
        {
            SetPlayers();
            SetFreeAgents();
            players[9].position = Position.WideReceiver;
            var roster = new Roster(players);
            IESPNTeam espnTeam = new ESPNTeamTest(roster, freeAgents);
            var manager = new RosterManager(roster, espnTeam);

            Player starter = roster.starters[startID];
            Player newGuy = manager.GetSub(startID, 2); //sub out starter 0

            Assert.IsTrue(newGuy.IsNull() ^ success);
        }

        [TestCase(0, 10, true)]
        [TestCase(0, 11, false)]
        [TestCase(6, 9, true)]
        [TestCase(6, 14, true)]
        public void TestRosterSwap(int p1, int p2, bool success)
        {
            SetPlayers();
            var roster = new Roster(players);

            Player starter = roster.GetPlayer(p1);
            Player newGuy = roster.GetPlayer(p2);

            roster.SwapPlayers(starter, newGuy);

            Assert.AreNotEqual(starter, newGuy);

            Assert.AreNotEqual(starter, roster.GetPlayer(p1));

            Assert.AreEqual(newGuy, roster.GetPlayer(p1));
        }

        [Test]
        public void TestManageTeam()
        {
            SetPlayers();
            SetFreeAgents();
            int currWeek = 17;
            players[0].eligibility = Eligibility.Out;
            players[1].eligibility = Eligibility.Injured;
            players[5].byeWeek = currWeek;
            Player kyle = players[10];
            Player george = players[6];
            Player quinn = freeAgents[0];

            var roster = new Roster(players);
            var espnTeam = new ESPNTeamTest(roster, freeAgents);
            var manager = new RosterManager(roster, espnTeam);

            manager.ManageTeam(currWeek);

            Assert.AreEqual(manager.Roster.starters[0], kyle);
            Assert.AreEqual(manager.Roster.starters[5], george);
            Assert.AreEqual(manager.Roster.starters[1], quinn);
        }

        [Test]
        public void TestLoginInfo()
        {
            string username = LoginInfo.GetUsername();
            string password = LoginInfo.GetPassword();

            Assert.AreEqual(username, "jjewett@bcsmail.org");
        }
    }
}
