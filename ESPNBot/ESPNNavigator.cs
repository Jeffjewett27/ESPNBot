using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using NLog;

namespace ESPNBot
{
    /// <summary>
    /// Performs the actions to navigate espn.com
    /// </summary>
    class ESPNNavigator
    {
        //Maps the positions to their espn abbrieviations
        private static Dictionary<Position, string> positionMap = new Dictionary<Position, string>()
        {
            { Position.Quarterback, "QB" },
            { Position.RunningBack, "RB" },
            { Position.WideReceiver, "WR" },
            { Position.TightEnd, "TE" },
            { Position.Defense, "D/ST" },
            { Position.Kicker, "K" },
            { Position.Flex, "FLEX" }
        };

        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private ChromeDriver driver;
        private WebDriverWait wait;
        private BrowserWait browserWait;

        //defines whether a table represents the roster page or the free agent page (elements are in different orders)
        public enum PlayerTable
        {
            Roster,
            FreeAgent
        }

        public ESPNNavigator(ChromeDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(4));
            browserWait = new BrowserWait(1000, 450);
        }

        /// <summary>
        /// Gets a table of rows representing players
        /// </summary>
        /// <returns></returns>
        public IWebElement GetTable()
        {
            return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                By.XPath("//tbody[@class='Table2__tbody']")));
        }

        /// <summary>
        /// Gets the nth table of rows representing players
        /// </summary>
        /// <param name="n">The table to select, from top of the page to bottom</param>
        /// <returns></returns>
        public IWebElement GetTable(int n)
        {
            var tables = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.PresenceOfAllElementsLocatedBy(
                By.XPath("//tbody[@class='Table2__tbody']")));
            if (n < tables.Count)
            {
                return tables[n];
            }
            else
            {
                logger.Error("Table " + n + " does not reference any table on the page");
                throw new ArgumentOutOfRangeException("Table " + n + " does not reference any table on the page");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Whether a table exists on the screen</returns>
        public bool TableExists()
        {
            return TableExists(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">The table to check</param>
        /// <returns>Whether the nth table exists on the screen</returns>
        public bool TableExists(int n)
        {
            try
            {
                var tables = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.PresenceOfAllElementsLocatedBy(
                    By.XPath("//tbody[@class='Table2__tbody']")));
                return tables.Count > n;
            } catch
            {
                return false;
            }
            
        }

        /// <summary>
        /// Gets the row from a table with a particular index
        /// </summary>
        /// <param name="table">The table to search</param>
        /// <param name="idx">The index of the row to find</param>
        /// <returns></returns>
        public IWebElement GetRow(IWebElement table, int idx)
        {
            return table.FindElement(By.XPath($"./tr[@data-idx='{idx}']"));
        }

        /// <summary>
        /// Gets the row from a table by searching for a player
        /// </summary>
        /// <param name="table">The table to search</param>
        /// <param name="player">The player to find</param>
        /// <param name="type">The type of tabler</param>
        /// <returns></returns>
        public IWebElement GetRow(IWebElement table, Player player, PlayerTable type)
        {
            var rows = table.FindElements(By.XPath($"./tr"));
            foreach (var row in rows)
            {
                Player p = ReadPlayerRow(row, type);
                if (p.name == player.name)
                {
                    return row;
                }
            }
            logger.Error("Player " + player + " not found in table");
            throw new NotFoundException("Player " + player + " not found in table");
        }

        /// <summary>
        /// Reads the player information from a slot in a table
        /// </summary>
        /// <param name="table">The table to read from</param>
        /// <param name="id">The id of the row to read from</param>
        /// <param name="type">The type of table</param>
        /// <returns></returns>
        public Player ReadPlayerRow(IWebElement table, int id, PlayerTable type)
        {
            var row = GetRow(table, id);
            return ReadPlayerRow(row, type);
        }

        /// <summary>
        /// Reads the player information from a row
        /// </summary>
        /// <param name="row">The row to read from</param>
        /// <param name="type">The type of row</param>
        /// <returns></returns>
        public Player ReadPlayerRow(IWebElement row, PlayerTable type)
        {
            int pBioBoxDivNum = 0;
            int projDivNum = 0;
            bool isMovable = false;
            switch (type)
            {
                case PlayerTable.FreeAgent:
                    pBioBoxDivNum = 1;
                    projDivNum = 6;
                    isMovable = IsFreeAgentClickable(row, true);
                    break;
                case PlayerTable.Roster:
                    pBioBoxDivNum = 2;
                    projDivNum = 6;
                    isMovable = IsRosterClickable(row);
                    break;
            }
            var pBioBox = row.FindElement(By.XPath($"td[{pBioBoxDivNum}]/div/div/div[2]/div"));
            var pBio = ReadPlayerBioBox(pBioBox);
            

            string proj = row.FindElement(By.XPath($"./td[{projDivNum}]/div/span")).Text;
            Double.TryParse(proj, out double projected);

            return new Player(pBio.Item1, pBio.Item2, pBio.Item3, pBio.Item4, projected, isMovable);
        }

        /// <summary>
        /// Gets the following player information from a player bio: name, team, eligibility, position
        /// </summary>
        /// <param name="pBioBox">The bioBox to read from</param>
        /// <returns>A tuple in the form (name, team, eligibility, position)</returns>
        private Tuple<string, string, string, string> ReadPlayerBioBox(IWebElement pBioBox)
        {
            string elg = "AOK";
            string name = pBioBox.FindElement(By.XPath("./div[1]")).GetAttribute("title");
            var nameSpans = pBioBox.FindElements(By.XPath("./div[1]/span"));
            if (nameSpans.Count == 3)
            {
                elg = nameSpans[1].GetAttribute("title");
            }
            var detailSpan = pBioBox.FindElements(By.XPath("./div[2]/span"));
            string team = detailSpan[0].Text;
            string pos = detailSpan[1].Text;
            return new Tuple<string, string, string, string>(name, team, elg, pos);
        }

        /// <summary>
        /// Clicks the tab for free agents of a position
        /// </summary>
        /// <param name="pos"></param>
        public void ClickPositionTab(Position pos)
        {
            positionMap.TryGetValue(pos, out string posAbbr);

            IWebElement posTab;
            try
            {
                posTab = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                    By.XPath($"//div[@id='filterSlotIds']/label[text()='{posAbbr}']")));
            } catch
            {
                throw new NotFoundException("The position tab for " + pos.ToString() + " could not be found");
            }
            ScrollElementIntoView(posTab);
            posTab.Click();
            browserWait.Wait();
        }

        /// <summary>
        /// Clicks the free agent action button
        /// </summary>
        /// <param name="row">The row of the button</param>
        public void ClickFreeAgentAction(IWebElement row)
        {
            IWebElement buttonNav = row.FindElement(By.XPath("./td[3]/div/div/div/button[1]"));
            ScrollElementIntoView(buttonNav);
            buttonNav.Click();
            browserWait.Wait();
        }

        /// <summary>
        /// Clicks the roster action button
        /// </summary>
        /// <param name="row">The row of the button</param>
        public void ClickRosterAction(IWebElement row)
        {
            var buttonNav = row.FindElement(By.XPath("./td[3]/div/div/button"));
            ScrollElementIntoView(buttonNav);
            buttonNav.Click();
            browserWait.Wait();
            
        }

        /// <summary>
        /// Checks whether the free agent action button is clickable
        /// </summary>
        /// <param name="row">The row of the button</param>
        /// <param name="useWaivers">Whether to accept waivers as valid</param>
        /// <returns></returns>
        public bool IsFreeAgentClickable(IWebElement row, bool useWaivers)
        {
            try
            {
                if (useWaivers)
                {
                    row.FindElement(By.XPath("./td[3]/div/div/div/button[1]"));
                }
                else
                {
                    row.FindElement(By.XPath("./td[3]/div/div/div/button[@title='Add']"));
                }
            } catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks whether the roster action button is clickable
        /// </summary>
        /// <param name="row">The row of the button</param>
        /// <returns></returns>
        public bool IsRosterClickable(IWebElement row)
        {
            try
            {
                row.FindElement(By.XPath("./td[3]/div/div/button"));
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Logs in to espn.com
        /// </summary>
        public void LogIn()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(4));
            var profileElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                By.XPath("//*[@id='espn-analytics']/div/div[2]/div/div/nav/ul[3]/li[2]/div[1]")));
            var loginElement = driver.FindElement(By.XPath("//*[@id='espn-analytics']/div/div[2]/div/div/nav/ul[3]/li[2]/div[2]/div/div/ul/li[3]"));
            var executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript("arguments[0].click();", loginElement);
            browserWait.Wait();
            driver.SwitchTo().Frame("disneyid-iframe");
            var usernameElement = driver.FindElement(By.XPath("//input[@placeholder='Username or Email Address']"));
            usernameElement.SendKeys(LoginInfo.GetUsername());
            browserWait.Wait();
            var passwordElement = driver.FindElement(By.XPath("//input[@placeholder='Password (case sensitive)']"));
            passwordElement.SendKeys(LoginInfo.GetPassword());
            passwordElement.SendKeys(Keys.Enter);

            browserWait.Wait();

            driver.Navigate().Refresh();
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                By.XPath("//*[@id='espn-analytics']/div/div[2]/div/div/nav/ul[3]/li[2]/div[1]")));
        }

        /// <summary>
        /// Clicks the continue button
        /// </summary>
        public void ClickContinue()
        {
            var continueButton = driver.FindElement(By.XPath("//button/span[text()='Continue']"));
            ScrollElementIntoView(continueButton);
            continueButton.Click();
            browserWait.Wait();
        }

        /// <summary>
        /// Clicks the cancel button
        /// </summary>
        public void ClickCancel()
        {
            var cancelButton = driver.FindElement(By.XPath("//button/span[text()='Cancel']"));
            ScrollElementIntoView(cancelButton);
            cancelButton.Click();
            browserWait.Wait();
        }

        /// <summary>
        /// Clicks the confirm button
        /// </summary>
        public void ClickConfirm()
        {
            var confirmButton = driver.FindElement(By.XPath("//button/span[text()='Confirm']"));
            ScrollElementIntoView(confirmButton);
            confirmButton.Click();
            browserWait.Wait();
        }

        /// <summary>
        /// Scrolls an element to the center of the screen to make it clickable
        /// </summary>
        /// <param name="element">The element to center</param>
        public void ScrollElementIntoView(IWebElement element)
        {
            var executor = (IJavaScriptExecutor)driver;
            string getScrollValue = @"
                var elY = arguments[0].getBoundingClientRect().y;
                var winH = window.innerHeight;
                var scroll = elY - winH / 2;
                window.scrollBy(0, scroll);";

            executor.ExecuteScript(getScrollValue, element);
            browserWait.Wait();
        }
    }
}
