using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using NLog;

namespace ESPNBot
{
    class ESPNTeam : IESPNTeam, IDisposable
    {
        private const string ROSTER_URL = "https://fantasy.espn.com/football/team?leagueId=61483480&teamId=10";
        private const string FREE_AGENTS_URL = "https://fantasy.espn.com/football/players/add?leagueId=61483480";
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private ChromeDriver driver;
        private ESPNNavigator navigator;
        private string url;
        private bool isLoggedIn;
        private Roster roster;

        public ESPNTeam()
        {
            StartBrowser();
            //url = ROSTER_URL;
            //driver.Url = url;
            url = "https://fantasy.espn.com/football/team?leagueId=61483480&teamId=10&seasonId=2019&scoringPeriodId=4&statSplit=singleScoringPeriod";
            driver.Url = url;
            navigator = new ESPNNavigator(driver);
            //LogIn();
        }

        ~ESPNTeam()
        {
            //CloseBrowser();
        }

        public void AddFreeAgent(Position pos, int slot)
        {
            if (url != FREE_AGENTS_URL)
            {
                url = FREE_AGENTS_URL;
                driver.Url = url;
            }
            if (!isLoggedIn)
            {
                LogIn();
            }

            navigator.ClickPositionTab(pos);

            var table = navigator.GetTable();

            Player best = null;
            int bestId = 0;
            for (int i = 0; i < 10; i++)
            {
                Player p = navigator.ReadPlayerRow(table, i, ESPNNavigator.PlayerTable.FreeAgent);
                if (best == null || p.CompareTo(best) > 0)
                {
                    best = p;
                    bestId = i;
                }
            }
            var bestRow = navigator.GetRow(table, bestId);
            navigator.ClickFreeAgentAction(bestRow);

            //then proceeed to swap with player in slot
        }

        public Player GetPlayer(int slot)
        {
            if (roster == null)
            {
                ReadRoster();
            }
            return roster.GetPlayer(slot);
        }

        public Player[] GetPlayers()
        {
            if (roster == null)
            {
                ReadRoster();
            }
            return roster.GetPlayers();
        }

        public void SwapPlayers(int s1, int s2)
        {
            logger.Info($"Swapping Players {s1} and {s2}");
            if (!isLoggedIn)
            {
                LogIn();
            }
            s1 += s1 >= 9 ? 1 : 0; //skip the 9th element
            s2 += s2 >= 9 ? 1 : 0;

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(4));
            var table = navigator.GetTable();
            
            var td1 = navigator.GetRow(table, s1);
            Thread.Sleep(1000);
            if (navigator.IsRosterClickable(td1))
            {
                navigator.ClickRosterAction(td1);
            } else
            {
                throw new ArgumentException("Row " + s1 + " cannot be moved");
            }

            var td2 = navigator.GetRow(table, s2);
            Thread.Sleep(1000);
            if (navigator.IsRosterClickable(td2))
            {
                navigator.ClickRosterAction(td2);
            }
            else
            {
                navigator.ClickRosterAction(td1);
                throw new ArgumentException("Row " + s2 + " cannot be moved");
            }

            Thread.Sleep(3000);
        }

        public void UpdatePlayer(Player player)
        {
            throw new NotImplementedException();
        }

        private void LogIn()
        {
            logger.Info("Logged in");
            navigator.LogIn();
            isLoggedIn = true;
        }

        private void ReadRoster()
        {
            if (url != ROSTER_URL)
            {
                url = ROSTER_URL;
                driver.Url = url;
            }
            if (!isLoggedIn)
            {
                LogIn();
            }

            Player[] players = new Player[16];
            var table = navigator.GetTable();
            foreach(var row in table.FindElements(By.XPath("./tr")))
            {
                Int32.TryParse(row.GetAttribute("data-idx"), out int idx);
                if (idx == 9) continue;
                if (idx > 9) idx--;
                players[idx] = navigator.ReadPlayerRow(row, ESPNNavigator.PlayerTable.Roster);
            }

            roster = new Roster(players);
        }

        private void StartBrowser()
        {
            driver = new ChromeDriver(@"C:\Program Files");
        }

        private void CloseBrowser()
        {
            driver.Close();
        }

        public void Dispose()
        {
            CloseBrowser();
            GC.SuppressFinalize(this);
        }
    }
}
