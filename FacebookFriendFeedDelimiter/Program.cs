using System;
using System.Security;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using LogLevel = NLog.LogLevel;

namespace FacebookFriendFeedDelimiter
{
    public static class Program
    {
        private static string UserName { get; set; }
        private static SecureString Password { get; set; }

        public static void Main(string[] args)
        {
            var log               = LogManager.GetLogger("primary");
            try
            {
                using (var chrome = new ChromeDriver())
                {
                    chrome.Url    = "https://www.facebook.com";

                    var loginForm = chrome.FindElementById("login_form");
                    if (loginForm != null)
                        Login(loginForm);

                    StartDeFriendFeed(chrome);
                }
            }
            catch (Exception exp)
            {
                log.Log(LogLevel.Error, exp);
            }
        }

        private static void Login(IWebElement element)
        {
            if(string.IsNullOrEmpty(UserName) || Password == null)
            {
                GetCredentials();
            }

            var loginBox = element.FindElement(By.Id("email"));
            if(loginBox == null)
                throw new NullReferenceException("There was an issue locating the login box, in the main login form. Check your URL or input and try again.");
            var passBox  = element.FindElement(By.Id("pass"));
            if (passBox == null)
                throw new NullReferenceException("There was an issue locating the password box, in the main login form. Check your URL or input and try again.");

            loginBox.SendKeys(UserName);
            passBox.SendKeys(Password.ConvertToInsecureString());
            element.Submit();
        }

        private static void GetCredentials()
        {
            if(Properties.Settings.Default.SavePassword
               && !string.IsNullOrEmpty(Properties.Settings.Default.UserName)
               && !string.IsNullOrEmpty(Properties.Settings.Default.Password))
            {
                GetStoredCredentials();
                return;
            }
            Console.WriteLine("----- Need Current FB Credentials -----");
            Console.WriteLine("Enter FB username: ");
            UserName                                         = Console.ReadLine();
            Console.WriteLine("Enter FB password: ");
            Password                                         = GetConsoleSecurePassword();
            if (Properties.Settings.Default.SavePassword)
                return;
            Console.WriteLine("Do you want to save your credentials?:[Yes/No]");
            var res                                          = Console.ReadLine();
            if (string.IsNullOrEmpty(res))
                return;
            switch (res.Trim().ToLower())
            {
                case "yes":
                case "y":
                    Properties.Settings.Default.SavePassword = true;
                    Properties.Settings.Default.UserName     = UserName;
                    Properties.Settings.Default.Password     = Password.EncryptString();
                    Properties.Settings.Default.Save();
                    break;
                case "no":
                case "n":
                    Properties.Settings.Default.SavePassword = false;
                    Properties.Settings.Default.UserName     = string.Empty;
                    Properties.Settings.Default.Password     = string.Empty;
                    Properties.Settings.Default.Save();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void GetStoredCredentials()
        {
            UserName = Properties.Settings.Default.UserName;
            Password = Properties.Settings.Default.Password.DecryptString();
        }

        private static SecureString GetConsoleSecurePassword()
        {
            var pwd   = new SecureString();
            while (true)
            {
                var i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }

                if (i.Key == ConsoleKey.Backspace)
                {
                    pwd.RemoveAt(pwd.Length - 1);
                    Console.Write("\b \b");
                }
                else
                {
                    pwd.AppendChar(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd;
        }

        private static void StartDeFriendFeed(ISearchContext webDriver)
        {
            var peopleYouMayKnow = webDriver.FindElements(By.XPath("//i[@class='img sp_88nz5MexVSt sx_acad75']"));
            foreach(var people in peopleYouMayKnow)
            {
                people.Click();
                System.Threading.Thread.Sleep(new TimeSpan(0,0,0,1));
            }
        }
    }
}
