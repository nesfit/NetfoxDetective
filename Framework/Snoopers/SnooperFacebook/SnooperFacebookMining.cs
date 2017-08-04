// Copyright (c) 2017 Jan Pluskal, Tomas Bruckner
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Netfox.SnooperFacebook.ModelsMining.Events;
using Netfox.SnooperFacebook.ModelsMining.Person;
using Netfox.SnooperFacebook.ModelsMining.Photos;
using Netfox.SnooperFacebook.ModelsMining.Places;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;

namespace Netfox.SnooperFacebook
{
    /// <summary>
    /// Class for data mining Facebook users. Needs connection to internet.
    /// </summary>
    public class SnooperFacebookMining
    {
        public int Iteration { get; set; } = 25;

        public string Email { get; set; } = "testtesttest22@seznam.cz";

        public string Password { get; set; } = "testtest";

        private PhantomJSDriver _driver;

        // connected to Facebook
        public bool IsLoggedIn { get; private set; }

        /// <summary>
        /// Creates WebDriver and logs in Facebook using default account
        /// </summary>
        public SnooperFacebookMining(bool setDriver)
        {
            if(!setDriver) { return; }
            this.SetDriver();
            this.LogInFacebook();
        }

        /// <summary>
        /// Creates WebDriver and logs in Facebook using specified account
        /// </summary>
        public SnooperFacebookMining(string email, string password)
        {
            this.Email = email;
            this.Password = password;
            this.SetDriver();
            this.LogInFacebook();
        }

        /// <summary>
        /// Quits webdriver (ends instance of phantomjs browser)
        /// </summary>
        public void QuitMining()
        {
            this._driver?.Quit();
            this._driver = null;
            this.IsLoggedIn = false;
        }

        /// <summary>
        /// Singleton method for webdriver
        /// </summary>
        /// <returns>
        /// True if new driver has been created
        /// False if driver is already created
        /// </returns>
        public bool SetDriver()
        {
            if (this._driver != null) { return false; }

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            this._driver = new PhantomJSDriver(driverService);
            return true;
        }

        /// <summary>
        /// Connects to Facebook using instance email and password properties
        /// </summary>
        /// <returns>
        /// True if you are logged into Facebook
        /// False if login failed
        /// </returns>
        public bool LogInFacebook()
        {
            this.SetDriver();

            this._driver.Navigate().GoToUrl("https://www.facebook.com");
            this._driver.FindElement(By.Id("email")).SendKeys(this.Email);
            this._driver.FindElement(By.Id("pass")).SendKeys(this.Password);
            try
            {
                this._driver.FindElement(By.Id("loginbutton")).Click();
            }
            catch
            {
                ((IJavaScriptExecutor)this._driver).ExecuteScript("window.stop();");
                this.IsLoggedIn = false;
                return false;
            }
            this.IsLoggedIn = true;
            return true;
        }

        /// <summary>
        /// Parses all targeted user's friends
        /// </summary>
        /// <param name="user">Targeted username</param>
        /// <returns>All targeted user's friends</returns>
        public FacebookFriendlist GetFriendlist(string user)
        {
            //this._driver.Navigate().GoToUrl(string.Format("https://www.facebook.com/profile.php?id={0}&sk=friends", 1084537908));
            this._driver.Navigate().GoToUrl(string.Format("https://www.facebook.com/{0}/friends", user));

            // getting all friends from Facebook infinite scroll
            var actualElementCount = this._driver.FindElements(By.CssSelector("ul")).Count;
            int previousElementCount;
            do
            {
                previousElementCount = actualElementCount;
                ((IJavaScriptExecutor) this._driver).ExecuteScript("scrollBy(0,3000)");
                Thread.Sleep(3000);
                actualElementCount = this._driver.FindElements(By.CssSelector("ul")).Count;
            } while(actualElementCount != previousElementCount);

            var elements = this._driver.FindElements(By.CssSelector("a[href*='friends_tab']"));
            return ParseFriendlist(elements, user);
        }

        /// <summary>
        /// Parses usernames from urls.
        /// </summary>
        /// <param name="friendLinkList"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private static FacebookFriendlist ParseFriendlist(IEnumerable<IWebElement> friendLinkList, string user)
        {
            var friendList = new FacebookFriendlist()
            {
                Username = user
            };

            // url contains username
            var regexNickname = new Regex(@"(https:\/\/www.facebook.com\/)(.*)(\?.*)");

            // url contains user's id
            var regexId = new Regex(@"(https:\/\/www.facebook.com\/profile\.php\?id=)(.*)(&fref.*)");
            foreach (var friend in friendLinkList)
            {
                var friendLink = friend.GetAttribute("href");
                var match = regexNickname.Match(friendLink);
                if (!match.Success) continue;
                var friendId = match.Groups[2].Value;

                // some users dont have nickname, only id
                if (friendId == "profile.php")
                {
                    match = regexId.Match(friendLink);
                    friendId = match.Groups[2].Value;
                }

                if (!IsDuplicate(friendList.Friends, friendId))
                {
                    friendList.Friends.Add(friendId);
                }
            }
            return friendList;
        }
    
        /// <summary>
        /// Checks for duplications in string array
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="newElement"></param>
        /// <returns></returns>
        private static bool IsDuplicate(IEnumerable<string> elements, string newElement)
        {
            return elements.Any(element => element == newElement);
        }

        /// <summary>
        /// Parses mutual friends from two friendlists
        /// </summary>
        /// <param name="userList1"></param>
        /// <param name="userList2"></param>
        /// <returns>List of mutual friends</returns>
        public FacebookMutualFriends GetMutualFriends(FacebookFriendlist userList1, FacebookFriendlist userList2)
        {
            var mutualFriends = new FacebookMutualFriends
            {
                MutualFriends = userList1.Friends.Where(userList2.Friends.Contains).ToList(),
            };
            mutualFriends.Participants.Add(userList1.Username);
            mutualFriends.Participants.Add(userList2.Username);
            return mutualFriends;
        }

        /// <summary>
        /// Parses mutual friends from mutual list and friendlist
        /// </summary>
        /// <param name="mutualList"></param>
        /// <param name="userList"></param>
        /// <returns>Mutual friends</returns>
        public FacebookMutualFriends GetMutualFriends(FacebookMutualFriends mutualList, FacebookFriendlist userList)
        {
            var mutualFriends = new FacebookMutualFriends
            {
                MutualFriends = mutualList.MutualFriends.Where(userList.Friends.Contains).ToList()
            };

            mutualFriends.Participants.Add(userList.Username);
            foreach (var participant in mutualList.Participants)
            {
                mutualFriends.Participants.Add(participant);
            }
            return mutualFriends;
        }

        /// <summary>
        /// Parses mutual friends from two mutual lists
        /// </summary>
        /// <param name="mutualList1"></param>
        /// <param name="mutualList2"></param>
        /// <returns>Mutual friends</returns>
        public FacebookMutualFriends GetMutualFriends(FacebookMutualFriends mutualList1, FacebookMutualFriends mutualList2)
        {
            var mutualFriends = new FacebookMutualFriends
            {
                MutualFriends = mutualList1.MutualFriends.Where(mutualList2.MutualFriends.Contains).ToList()
            };

            foreach (var participant in mutualList1.Participants)
            {
                mutualFriends.Participants.Add(participant);
            }

            foreach (var participant in mutualList2.Participants)
            {
                mutualFriends.Participants.Add(participant);
            }

            return mutualFriends;
        }

        /// <summary>
        /// Parses public information about specified user
        /// </summary>
        /// <param name="user">User's username</param>
        /// <returns>User's public information</returns>
        public FacebookPublicInfo GetFacebookPublicInfo(string user)
        {
            var webClient = new WebClient();
            string downloadedString;

            try
            {
                downloadedString = webClient.DownloadString(@"http://graph.facebook.com/" + user);
            }
            catch(Exception) {
                return null;
            }
            

            var userPublicInfoJson = JObject.Parse(downloadedString);

            if (userPublicInfoJson["error"] != null)
            {
                // invalid username
                return null;
            }

            var facebookPublicProfile = new FacebookPublicInfo
            {
                Gender = (string)userPublicInfoJson["gender"],
                Name = (string)userPublicInfoJson["name"],
                Username = (string)userPublicInfoJson["username"],
                Id = (ulong)userPublicInfoJson["id"],
                ProfileLink = (string)userPublicInfoJson["link"],
                Locale = (string)userPublicInfoJson["locale"]
            };

            return facebookPublicProfile;
        }

        /// <summary>
        /// Downloads user's profile picture (always public)
        /// </summary>
        /// <param name="user">User's username or id</param>
        /// <param name="filenamePath">where to download</param>
        /// <returns>
        /// True if download was successful
        /// False if download failed
        /// </returns>
        public bool GetProfilePicture(string user, string filenamePath)
        {
            // filenamePath example   D:\file.jpg
            try
            {
                var webClient = new WebClient();
                webClient.DownloadFile(@"http://graph.facebook.com/" + user + @"/picture?type=large", filenamePath);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Parses events that specified user will be going to
        /// </summary>
        /// <param name="user">User's username</param>
        /// <returns>List of upcoming events</returns>
        public IEnumerable<FacebookUpcomingEvent> GetUpcomingEvents(string user)
        {
            this._driver.Navigate().GoToUrl(string.Format("https://www.facebook.com/{0}/upcoming_events", user));
            var elements = this._driver.FindElements(By.CssSelector("a[href*='events']"));

            return (from element in elements
                    where element.Text != string.Empty
                    where !element.Text.Contains("\r")
                    let url = element.GetAttribute("href")
                    where !url.Contains(string.Format("{0}", user))
                    select new FacebookUpcomingEvent()
                    {
                        UserId = user,
                        Url = url,
                        Name = element.Text
                    }).ToList();
        }

        /// <summary>
        /// Parses events that specified user has visited
        /// </summary>
        /// <param name="user">User's username</param>
        /// <returns>List of visited events</returns>
        public IEnumerable<FacebookPastEvent> GetPastEvents(string user)
        {
            this._driver.Navigate().GoToUrl(string.Format("https://www.facebook.com/{0}/past_events", user));
            var elements = this._driver.FindElements(By.CssSelector("a[href*='events']"));

            return (from element in elements
                    where element.Text != string.Empty
                    where !element.Text.Contains("\r")
                    let url = element.GetAttribute("href")
                    where !url.Contains(string.Format("{0}", user))
                    select new FacebookPastEvent()
                    {
                        UserId = user,
                        Url = url,
                        Name = element.Text
                    }).ToList();
        }

        /// <summary>
        /// Parses album names of specified user
        /// </summary>
        /// <param name="user">User's username</param>
        /// <returns>List of album names</returns>
        public IEnumerable<FacebookAlbum> ParseAlbums(string user)
        {
            this._driver.Navigate().GoToUrl(string.Format("https://www.facebook.com/{0}/photos_albums", user));
            var elements = this._driver.FindElements(By.CssSelector("strong"));

            return (from element in elements
                    where element.Text != string.Empty
                    select new FacebookAlbum()
                    {
                        UserId = user,
                        Name = element.Text
                    }).ToList();
        }

        /// <summary>
        /// Parses places, that specified user recently visited
        /// </summary>
        /// <param name="user">User's username</param>
        /// <returns>List of recent places</returns>
        public IEnumerable<FacebookRecentPlace> GetRecentPlaces(string user)
        {
            this._driver.Navigate().GoToUrl(string.Format("https://www.facebook.com/{0}/places_recent", user));
            var elements = this._driver.FindElements(By.CssSelector("a[href*='pages']"));

            return (from element in elements
                    where element.Text != string.Empty
                    where !element.Text.Contains("Create")
                    select new FacebookRecentPlace()
                    {
                        UserId = user,
                        Name = element.Text,
                        Url = element.GetAttribute("href")
                    }).ToList();
        }
    }
}
