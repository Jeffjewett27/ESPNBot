using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

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

        private ChromeDriver driver;
        private WebDriverWait wait;

        public enum PlayerTable
        {
            Roster,
            FreeAgent
        }

        public ESPNNavigator(ChromeDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(4));
        }

        public IWebElement GetTable()
        {
            return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                By.XPath("//tbody[@class='Table2__tbody']")));
        }

        public IWebElement GetRow(IWebElement table, int idx)
        {
            return table.FindElement(By.XPath($"./tr[@data-idx='{idx}']"));
            /*try
            {
                
            }
            catch
            {
                throw new ArgumentException("Row " + idx + " in table could not be found");
            }*/
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
            switch (type)
            {
                case PlayerTable.FreeAgent:
                    pBioBoxDivNum = 1;
                    projDivNum = 6;
                    break;
                case PlayerTable.Roster:
                    pBioBoxDivNum = 2;
                    projDivNum = 6;
                    break;
            }
            var pBioBox = row.FindElement(By.XPath($"td[{pBioBoxDivNum}]/div/div/div[2]/div"));
            var pBio = ReadPlayerBioBox(pBioBox);


            string proj = row.FindElement(By.XPath($"./td[{projDivNum}]/div/span")).Text;
            Double.TryParse(proj, out double projected);

            return new Player(pBio.Item1, pBio.Item2, pBio.Item3, pBio.Item4, projected);
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
                By.XPath($"//div[@id='filterSlotIds']/label[text()='{posAbbr}'")));
            posTab.Click();
        }

        public void ClickFreeAgentAction(IWebElement row)
        {
            var buttonNav = row.FindElement(By.XPath("./td[3]/div/div/div/button[1]"));
            buttonNav.Click();
        }

        public void ClickRosterAction(IWebElement row)
        {
            var buttonNav = row.FindElement(By.XPath("./td[3]/div/div/button"));
            buttonNav.Click();
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

            Thread.Sleep(2000);

            driver.Navigate().Refresh();
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                By.XPath("//*[@id='espn-analytics']/div/div[2]/div/div/nav/ul[3]/li[2]/div[1]")));
        }
    }
}
