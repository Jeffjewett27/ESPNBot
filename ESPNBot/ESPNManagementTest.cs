﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;

namespace ESPNBot
{
    [TestFixture]
    class ESPNManagementTest
    {
        [Test]
        public void ESPNTeamTest()
        {
            using (var test = new ESPNTeam())
            {
                Logger l = new Logger();
                Player[] players = test.GetPlayers();
                Roster roster = new Roster(players);
                int count = players.Length;
                RosterManager manager = new RosterManager(roster, test);
                manager.ManageTeam(4);
            }
        }

        [Test]
        public void ESPNSwapTest()
        {
            using (var test = new ESPNTeam())
            {
                test.SwapPlayers(1, 9);
                Thread.Sleep(4000);
            }
        }

        [Test]
        public void NavigatorSwap()
        {
            var driver = new ChromeDriver(@"C:\Program Files")
            {
                Url = "https://fantasy.espn.com/football/team?leagueId=61483480&teamId=10&seasonId=2019"
            };

            var navigator = new ESPNNavigator(driver);
            navigator.LogIn();
            var table = navigator.GetTable();
            var row = navigator.GetRow(table, 1);
            navigator.ClickRosterAction(row);
            Thread.Sleep(1000);
            row = navigator.GetRow(table, 10);
            navigator.ClickRosterAction(row);

            driver.Close();
        }

        [Test]
        public void NavigatorFreeAgent()
        {
            var driver = new ChromeDriver(@"C:\Program Files")
            {
                Url = "https://fantasy.espn.com/football/team?leagueId=61483480&teamId=10&seasonId=2019"
            };

            var navigator = new ESPNNavigator(driver);
            navigator.LogIn();
            var table = navigator.GetTable();
            
        }
    }
}
