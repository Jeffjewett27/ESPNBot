using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using NLog;

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
                Logger.Configure();
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
            using (var test = new ESPNTeam())
            {
                test.AddFreeAgent(Position.TightEnd, new Player("Jordan Akins", "HOU", Eligibility.AOK, Position.TightEnd, 6.2, true), false);
            }
        }

        [Test]
        public void JavascriptTest()
        {
            ChromeDriver driver = new ChromeDriver(@"C:/Program Files");
            driver.Url = "https://fantasy.espn.com/football/team?leagueId=61483480&teamId=10";

            var executor = (IJavaScriptExecutor)driver;
            /*for (int i = 0; i < 4; i++)
            {
                executor.ExecuteScript("window.scrollBy(0,200)");
                Thread.Sleep(1000);
            }
            driver.Close();*/
            var navigator = new ESPNNavigator(driver);
            var table = navigator.GetTable();
            var row = navigator.GetRow(table, 15);
            navigator.ScrollElementIntoView(row);
            row = navigator.GetRow(table, 1);
            navigator.ScrollElementIntoView(row);
            driver.Close();
        }

        [Test]
        public void LoggerTest()
        {
            Logger.Configure();
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("hello world");
        }
    }
}
