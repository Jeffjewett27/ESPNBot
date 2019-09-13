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
    class WebTest
    {
        IWebDriver driver;

        [SetUp]
        public void StartBrowser()
        {
            //driver = new ChromeDriver("D:\\3rdparty\\chrome");
            //System.SetProperty("webdriver.chrome.driver", "D:\\Drivers\\chromedriver.exe");
            var options = new ChromeOptions
            {
                BinaryLocation = @"C:\Users\jeffr\Downloads\chromedriver_win32"
            };

            string current = Directory.GetCurrentDirectory();

            driver = new ChromeDriver(@"C:\Program Files");
        }

        [Test]
        public void PlaygroundTest()
        {
            driver.Url = "http://uitestingplayground.com/";
            string title = driver.Title;

            //var body = driver.FindElement(By.TagName("body"));
            //var overview = body.FindElement(By.Id("overview"));
            //var rows = overview.FindElements(By.ClassName("row"));

            //IWebElement clickButton = null;
            /*foreach(var row in rows)
            {
                foreach (var column in row.FindElements(By.ClassName("col-sm")))
                {
                    if (column.FindElement(By.TagName("a")).GetAttribute("href") == "http://uitestingplayground.com/click")
                    {
                        clickButton = column;
                    }
                }
            }*/

            var elements = driver.FindElements(By.XPath(".//body//section[@id='overview']//div[@class='container']//div[@class='row']"));

            string[][] links = new string[elements.Count][];
            int i = 0;
            IWebElement clickButton = null;
            foreach (var row in elements)
            {
                int j = 0;
                var columns = row.FindElements(By.XPath(".//div[@class='col-sm']//h3//a"));
                links[i] = new string[columns.Count];
                foreach (var column in columns)
                {
                    links[i][j] = column.Text + ", " + column.GetAttribute("href");
                    j++;

                    if (column.Text == "Click")
                    {
                        clickButton = column;
                    }
                }
                i++;
            }

            if (clickButton != null)
            {
                string name = clickButton.GetAttribute("href");

                clickButton.Click();
            }
            //Thread.Sleep(4000);
        }

        [Test]
        public void ESPNTest()
        {
            //driver.Url = "https://fantasy.espn.com/football/team?leagueId=61483480&teamId=10&seasonId=2019";
            driver.Url = "https://fantasy.espn.com/football/team?leagueId=61483480&teamId=10&seasonId=2019&scoringPeriodId=8&statSplit=singleScoringPeriod";

            var elements = driver.FindElements(By.XPath("//tr//td//div[@title='Opponent']"));
            //Table2__tr Table2__tr--lg Table2__odd
        }

        [TearDown]
        public void CloseBrowser()
        {
            driver.Close();
        }
    }
}
