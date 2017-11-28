using System;
using System.Threading;
using Nebula.OneTime.TimesheetModels;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Keys = OpenQA.Selenium.Keys;

namespace Nebula.OneTime.TimesheetBookingModels
{
    internal class FlexposureAfas
    {
        private readonly AltenTimeSheet _timesheetData;
        private readonly string _password;
        private readonly string _username;

        public FlexposureAfas(AltenTimeSheet timesheetData, string username, string password)
        {
            _timesheetData = timesheetData;
            _username = username;
            _password = password;
        }

        public void EnterHours()
        {
            using (var driver = new ChromeDriver { Url = "https://79406.afasinsite.nl/login?url=%2f" })
            {
                var year = _timesheetData.StartDate.ToString("yyyy");
                var month = _timesheetData.StartDate.ToString("MM");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                // Wait for login form to show
                wait.Until(d => d.FindElement(By.Id("P_C_W_A3DD0FF345E84C3CB3E8D9C23A8EDEBE_Content")).Displayed);

                // Login
                driver.FindElement(By.Id("P_C_W_A3DD0FF345E84C3CB3E8D9C23A8EDEBE_LoginUserName")).SendKeys(_username);
                driver.FindElement(By.Id("P_C_W_A3DD0FF345E84C3CB3E8D9C23A8EDEBE_LoginPassword")).SendKeys(_password);
                driver.FindElement(By.Id("P_C_W_A3DD0FF345E84C3CB3E8D9C23A8EDEBE_LiteralLoginWebPart_Login")).Click();

                // Wait for main page to show
                wait.Until(d => d.FindElement(By.Id("P_H_W_Login_LiteralLoginWebPart_OptionsMenu")).Displayed);

                // Move to projects
                driver.FindElement(By.Id("P_H_W_Menu_T_2_ctl01")).Click();

                // Wait for projects to show
                wait.Until(d => d.FindElement(By.Id("P_C_W_E0783A33426832248D310F9C9B674EE4_ctl03")).Displayed);

                // Go to book hours
                driver.FindElement(By.Id("P_C_W_E0783A33426832248D310F9C9B674EE4_ctl03")).Click();

                // Wait for book hours to show
                wait.Until(d => d.FindElement(By.Id("Window_0_Entry_Selection_Selection_Year")).Displayed);

                // Ensure that we are at the proper spot
                var yearInput = driver.FindElement(By.Id("Window_0_Entry_Selection_Selection_Year"));
                if (yearInput.Equals(driver.SwitchTo().ActiveElement()) == false)
                {
                    throw new InvalidElementStateException();
                }

                // Enter year
                driver.SwitchTo().ActiveElement().SendKeys($"{year}");
                GoToNextField(driver);

                // Enter Period
                driver.SwitchTo().ActiveElement().SendKeys($"{month}");
                GoToNextField(driver);

                // Enter Timesheet
                driver.SwitchTo().ActiveElement().SendKeys(Keys.Enter);
                
                // Enter row data
                for (int i = 0; i < _timesheetData.WorkingHours.Count; i++)
                {
                    if (_timesheetData.WorkingHours[i] > 0)
                    {
                        // Wait a bit (improve by counting the tr's)
                        Thread.Sleep(2000);

                        var day = _timesheetData.Days[i].ToString("00");
                        var projectNumber = "003572";

                        // Enter date
                        driver.SwitchTo().ActiveElement().SendKeys($"{day}-");
                        driver.SwitchTo().ActiveElement().SendKeys($"{month}-");
                        driver.SwitchTo().ActiveElement().SendKeys($"{year}");
                        GoToNextField(driver);

                        // Enter Project
                        driver.SwitchTo().ActiveElement().SendKeys($"{projectNumber}");
                        GoToNextField(driver);
                        GoToNextField(driver);
                        GoToNextField(driver);

                        // Enter hours
                        driver.SwitchTo().ActiveElement().SendKeys($"{_timesheetData.WorkingHours[i]}");
                        GoToNextField(driver);
                        GoToNextField(driver);

                        // Enter Code
                        driver.SwitchTo().ActiveElement().SendKeys("ONT");
                        GoToNextField(driver);
                        GoToNextField(driver);

                        // Enter hour sort
                        driver.SwitchTo().ActiveElement().SendKeys("N");
                        GoToNextField(driver);
                    }
                }

                // Go to save
                Thread.Sleep(2000);
                driver.FindElement(By.Id("P_C_W_Entry_Actions_E0_ButtonEntryWebPart_OK_E0")).Click();

                // Logout
                Thread.Sleep(2000);
                driver.FindElement(By.Id("P_H_W_Login_LiteralLoginWebPart_OptionsMenu")).Click();
                driver.FindElement(By.Id("P_H_W_Login_Logout")).Click();
            }
        }
        private void GoToNextField(IWebDriver driver)
        {
            Thread.Sleep(100);
            driver.SwitchTo().ActiveElement().SendKeys(Keys.Tab);
            Thread.Sleep(100);
        }
    }
}
