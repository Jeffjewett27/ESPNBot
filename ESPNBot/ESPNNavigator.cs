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
    class ESPNNavigator
    {
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

        public IWebElement GetTable()
        {
            return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                By.XPath("//tbody[@class='Table2__tbody']")));
        }

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

        public bool TableExists()
        {
            return TableExists(0);
        }

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

        public IWebElement GetRow(IWebElement table, int idx)
        {
            return table.FindElement(By.XPath($"./tr[@data-idx='{idx}']"));
        }

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

        public Player ReadPlayerRow(IWebElement table, int id, PlayerTable type)
        {
            var row = GetRow(table, id);
            return ReadPlayerRow(row, type);
        }

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
                    isMovable = IsFreeAgentClickable(row);
                    break;
                case PlayerTable.Roster:
                    pBioBoxDivNum = 2;
                    projDivNum = 6;
                    isMovable = IsFreeAgentClickable(row);
                    break;
            }
            var pBioBox = row.FindElement(By.XPath($"td[{pBioBoxDivNum}]/div/div/div[2]/div"));
            var pBio = ReadPlayerBioBox(pBioBox);
            

            string proj = row.FindElement(By.XPath($"./td[{projDivNum}]/div/span")).Text;
            Double.TryParse(proj, out double projected);

            return new Player(pBio.Item1, pBio.Item2, pBio.Item3, pBio.Item4, projected, isMovable);
        }

        public Tuple<string, string, string, string> ReadPlayerBioBox(IWebElement pBioBox)
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

        public void ClickPositionTab(Position pos)
        {
            positionMap.TryGetValue(pos, out string posAbbr);

            var posTab = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                By.XPath($"//div[@id='filterSlotIds']/label[text()='{posAbbr}']")));
            ScrollElementIntoView(posTab);
            posTab.Click();
            browserWait.Wait();
        }

        public void ClickFreeAgentAction(IWebElement row)
        {
            var buttonNav = row.FindElement(By.XPath("./td[3]/div/div/div/button[1]"));
            ScrollElementIntoView(buttonNav);
            buttonNav.Click();
            browserWait.Wait();
        }

        public void ClickRosterAction(IWebElement row)
        {
            var buttonNav = row.FindElement(By.XPath("./td[3]/div/div/button"));
            ScrollElementIntoView(buttonNav);
            buttonNav.Click();
            browserWait.Wait();
            
        }

        public bool IsFreeAgentClickable(IWebElement row)
        {
            try
            {
                row.FindElement(By.XPath("./td[3]/div/div/div/button[@title='Add']"));
            } catch
            {
                return false;
            }
            return true;
        }

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

        public void LogIn()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(4));
            var profileElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                By.XPath("//*[@id='espn-analytics']/div/div[2]/div/div/nav/ul[3]/li[2]/div[1]")));
            Actions action = new Actions(driver);
            action.MoveToElement(profileElement).Perform();
            Thread.Sleep(100);
            action.MoveByOffset(0, 200).MoveByOffset(0, 0).Perform();
            Thread.Sleep(200);
            var loginElement = driver.FindElement(By.XPath("//*[@id='espn-analytics']/div/div[2]/div/div/nav/ul[3]/li[2]/div[2]/div/div/ul/li[3]"));
            action.MoveToElement(loginElement).Perform();
            Thread.Sleep(100);
            action.Click(loginElement).Perform();
            Thread.Sleep(500);
            driver.SwitchTo().Frame("disneyid-iframe");
            var usernameElement = driver.FindElement(By.XPath("//input[@placeholder='Username or Email Address']"));
            usernameElement.SendKeys(LoginInfo.GetUsername());
            Thread.Sleep(150);
            var passwordElement = driver.FindElement(By.XPath("//input[@placeholder='Password (case sensitive)']"));
            passwordElement.SendKeys(LoginInfo.GetPassword());
            passwordElement.SendKeys(Keys.Enter);

            browserWait.Wait();

            driver.Navigate().Refresh();
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                By.XPath("//*[@id='espn-analytics']/div/div[2]/div/div/nav/ul[3]/li[2]/div[1]")));
        }

        public void ClickContinue()
        {
            var continueButton = driver.FindElement(By.XPath("//button/span[text()='Continue']"));
            ScrollElementIntoView(continueButton);
            continueButton.Click();
            browserWait.Wait();
        }

        public void ClickCancel()
        {
            var cancelButton = driver.FindElement(By.XPath("//button/span[text()='Cancel']"));
            ScrollElementIntoView(cancelButton);
            cancelButton.Click();
            browserWait.Wait();
        }

        public void ClickConfirm()
        {
            var confirmButton = driver.FindElement(By.XPath("//button/span[text()='Confirm']"));
            ScrollElementIntoView(confirmButton);
            confirmButton.Click();
            browserWait.Wait();
        }

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
