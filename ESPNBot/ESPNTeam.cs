﻿using OpenQA.Selenium;
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

        public void AddFreeAgent(Position pos, Player dropPlayer)
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
            logger.Info("Selected free agent " + best);
            var bestRow = navigator.GetRow(table, bestId);
            navigator.ClickFreeAgentAction(bestRow);

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

            navigator.ClickRosterAction(row);
            navigator.ClickContinue();
            navigator.ClickConfirm();
            logger.Info(dropPlayer + "successfully replaced by " + best);
            
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

        public void UpdatePlayer(Player player)
        {
            throw new NotImplementedException();
        }

        private void LogIn()
        {
            navigator.LogIn();
            isLoggedIn = true;
            logger.Info("Logged in");
        }

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
            StringBuilder rosterString = new StringBuilder("Roster read:");
            foreach (Player p in roster.GetPlayers())
            {
                rosterString.Append("\n");
                rosterString.Append(p);
            }
            logger.Info(rosterString.ToString());
        }

        private void StartBrowser()
        {
            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("headless");
            driver = new ChromeDriver(@"C:\Program Files", options);
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
