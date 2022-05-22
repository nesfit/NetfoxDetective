// Copyright (c) 2017 Jan Pluskal
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

namespace Netfox.AppIdent.Misc
{
    public class Entropy
    {
        public static double Calculate<T>(IEnumerable<EntropyItem<T>> items)
        {
            var classes = 0;
            return Calculate(items, out classes);
        }

        public static double Calculate<T>(IEnumerable<EntropyItem<T>> items, out int classes)
        {
            var enthropyItems = items as EntropyItem<T>[] ?? items.ToArray();
            classes = enthropyItems.Sum(item => item.Count);
            var enth = 0.0;
            foreach(var item in enthropyItems)
            {
                var px = item.Count / (double) classes;
                enth -= (px * Math.Log(px));
            }
            return enth;
        }

        public static double Calculate<T>(IEnumerable<T> items)
        {
            var classes = 0;
            return Calculate(items, out classes);
        }

        public static double Calculate<T>(IEnumerable<T> items, out int classes)
        {
            var histogram = items.GroupBy(item => item).Select(g => new EntropyItem<T>
            {
                Key = g.Key,
                Count = g.Count()
            });
            return Calculate(histogram, out classes);
        }

        public class EntropyItem<T>
        {
            public T Key { get; set; }
            public int Count { get; set; }
        }
    }
}