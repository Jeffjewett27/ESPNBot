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
using static ESPNBot.ESPNNavigator;

namespace ESPNBot
{
    /// <summary>
    /// Manages the interface between roster actions and navigating espn.com
    /// </summary>
    class ESPNTeam : IESPNTeam, IDisposable
    {
        private const string ROSTER_URL = "https://fantasy.espn.com/football/team?leagueId=61483480&teamId=10"; //url to the roster page
        private const string FREE_AGENTS_URL = "https://fantasy.espn.com/football/players/add?leagueId=61483480"; //url to the free agent page
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private ChromeDriver driver;
        private ESPNNavigator navigator;
        private string url;
        private bool isLoggedIn;
        private Roster roster;

        public ESPNTeam()
        {
            StartBrowser();
            url = ROSTER_URL;
            driver.Url = url;
            navigator = new ESPNNavigator(driver);
        }

        /// <summary>
        /// Retrieves a free agent for a position and swaps him for a position
        /// </summary>
        /// <param name="pos">The position of the player to pick up</param>
        /// <param name="dropPlayer">The player to drop</param>
        /// <param name="useWaivers">Whether or not to include players from waivers</param>
        public void AddFreeAgent(Position pos, Player dropPlayer, bool useWaivers)
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

            //select new player
            var table = navigator.GetTable();
            Player best = null;
            int bestId = 0;
            for (int i = 0; i < 10; i++)
            {
                var pRow = navigator.GetRow(table, i);
                Player p = navigator.ReadPlayerRow(pRow, ESPNNavigator.PlayerTable.FreeAgent);
                bool isClickable = navigator.IsFreeAgentClickable(pRow, useWaivers);
                if (isClickable && (best == null || p.CompareTo(best) > 0))
                {
                    best = p;
                    bestId = i;
                }
            }
            logger.Info("Selected free agent " + best);
            var bestRow = navigator.GetRow(table, bestId);
            navigator.ClickFreeAgentAction(bestRow);

            //select player to drop
            int n = 1;
            IWebElement row = null;
            while (navigator.TableExists(n))
            {
                table = navigator.GetTable(n);
                try
                {
                    row = navigator.GetRow(table, dropPlayer, PlayerTable.Roster);
                    break;
                } catch { }
                n++;
            }

            if (row == null)
            {
                navigator.ClickCancel();
                logger.Error("Player " + dropPlayer + " could not be located to be dropped");
                throw new NotFoundException("Player " + dropPlayer + " could not be located to be dropped");
            }
            //confirm action
            navigator.ClickRosterAction(row);
            navigator.ClickContinue();
            navigator.ClickConfirm();
            logger.Info(dropPlayer + "successfully replaced by " + best);
        }

        /// <summary>
        /// Retrieves the player in the roster slot number
        /// </summary>
        /// <param name="slot">The slot to retrieve</param>
        /// <returns></returns>
        public Player GetPlayer(int slot)
        {
            if (roster == null)
            {
                ReadRoster();
            }
            return roster.GetPlayer(slot);
        }

        /// <summary>
        /// Returns the array of players from espn.com
        /// </summary>
        /// <returns></returns>
        public Player[] GetPlayers()
        {
            if (roster == null)
            {
                ReadRoster();
            }
            return roster.GetPlayers();
        }

        /// <summary>
        /// Swaps players from the espn.com roster
        /// </summary>
        /// <param name="s1">The first player slotID to swap</param>
        /// <param name="s2">The second player slotID to swap</param>
        public void SwapPlayers(int s1, int s2)
        {
            if (s1 == s2)
            {
                logger.Error("Attempted to swap slot " + s1 + "with itself");
                throw new ArgumentOutOfRangeException("Cannot swap slot " + s1 + " with itself");
            }
            logger.Info($"Swapping Players {s1} and {s2}");
            if (!isLoggedIn)
            {
                LogIn();
            }
            s1 += s1 >= 9 ? 1 : 0; //skip the 9th element, which is a blank row
            s2 += s2 >= 9 ? 1 : 0;

            var table = navigator.GetTable();
            var td1 = navigator.GetRow(table, s1);
            if (navigator.IsRosterClickable(td1))
            {
                navigator.ClickRosterAction(td1);
            } else
            {
                logger.Error("Row " + s1 + " cannot be moved");
                throw new ArgumentException("Row " + s1 + " cannot be moved");
            }

            var td2 = navigator.GetRow(table, s2);
            if (navigator.IsRosterClickable(td2))
            {
                navigator.ClickRosterAction(td2);
            }
            else
            {
                navigator.ClickRosterAction(td1);
                logger.Error("Row " + s2 + " cannot be moved");
                throw new ArgumentException("Row " + s2 + " cannot be moved");
            }
        }

        /// <summary>
        /// Updates the information of a player
        /// </summary>
        /// <param name="player">The player to update</param>
        public Player UpdatePlayer(Player player)
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
            int slot = roster.GetPlayerSlot(player);
            var table = navigator.GetTable();
            var row = navigator.GetRow(table, player, PlayerTable.Roster);
            Player updated = navigator.ReadPlayerRow(row, PlayerTable.Roster);
            roster.ReplacePlayer(slot, updated);
            return updated;
        }
        
        /// <summary>
        /// Logs in to espn.com
        /// </summary>
        private void LogIn()
        {
            navigator.LogIn();
            isLoggedIn = true;
            logger.Info("Logged in");
        }

        /// <summary>
        /// Reads the roster from espn.com
        /// </summary>
        private void ReadRoster()
        {
            if (url != ROSTER_URL)
            {
                logger.Trace("Going to roster page");
                url = ROSTER_URL;
                driver.Url = url;
            }
            if (!isLoggedIn)
            {
                LogIn();
            }

            logger.Trace("Reading the roster");
            Player[] players = new Player[16];
            var table = navigator.GetTable();
            foreach(var row in table.FindElements(By.XPath("./tr")))
            {
                Int32.TryParse(row.GetAttribute("data-idx"), out int idx);
                if (idx == 9) continue;
                if (idx > 9) idx--;
                players[idx] = navigator.ReadPlayerRow(row, PlayerTable.Roster);
            }
            roster = new Roster(players);

            //for logging purposes only
            StringBuilder rosterString = new StringBuilder("Roster read:");
            foreach (Player p in roster.GetPlayers())
            {
                rosterString.Append("\n");
                rosterString.Append(p);
            }
            logger.Info(rosterString.ToString());
        }

        /// <summary>
        /// Opens the ChomeDriver in headless mode
        /// </summary>
        private void StartBrowser()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--window-size=1300,800");
            driver = new ChromeDriver(@"C:\Program Files", options);
        }

        /// <summary>
        /// Closes the driver
        /// </summary>
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
