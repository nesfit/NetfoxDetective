// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka
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
using Netfox.Snoopers.SnooperWebmails.Models.Spotters;

namespace Netfox.Snoopers.SnooperWebmails.Models.SpotterVisitors
{
    /// <summary>
    /// Manages implemented Spotter Visitors.
    /// Is aware of all implemented visitors. 
    /// Instatiate them only if they are needed and then holds their instance in Dictionary for future use.
    /// </summary>
    public static class SpotterVisitorsManager
    {

        private static Dictionary<string, ISpotterVisitor> _visitors = new Dictionary<string, ISpotterVisitor>();

        /// <summary>
        /// List of known Visitors.
        /// </summary>
        public static readonly List<string> KnownVisitors = new List<string>()
        {
            "Yahoo/GetListedMessages",
            "Yahoo/GetDisplayMessage",
            "Seznam/GetNewMessage",
            "Seznam/GetListedMessages",
            "Seznam/GetDisplayMessage"
        };

        /// <summary>
        /// Returns needed Visitor.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ISpotterVisitor GetSpotterVisitor(String name)
        {
            if(_visitors.ContainsKey(name)) return _visitors[name];
            else
            {
                switch(name)
                {
                    case "Yahoo/GetListedMessages":
                        var yl = new YahooGetListedMessages();
                        _visitors.Add("Yahoo/GetListedMessages",yl);
                        return yl;
                    case "Yahoo/GetDisplayMessage":
                        var yd = new YahooGetDisplayMessage();
                        _visitors.Add("Yahoo/GetDisplayMessage", yd);
                        return yd;
                    case "Seznam/GetNewMessage":
                        var sn = new SeznamGetNewMessage();
                        _visitors.Add("Seznam/GetNewMessage",sn);
                        return sn;
                    case "Seznam/GetListedMessages":
                        var sl = new SeznamGetListedMessages();
                        _visitors.Add("Seznam/GetListedMessages",sl);
                        return sl;
                    case "Seznam/GetDisplayMessage":
                        var sd = new SeznamGetDisplayMessage();
                        _visitors.Add("Seznam/GetDisplayMessage",sd);
                        return sd;
                    default:
                        throw new ArgumentOutOfRangeException("name", "Unknown spotter visitor "+name);
                }
            }
        }
    }
}
