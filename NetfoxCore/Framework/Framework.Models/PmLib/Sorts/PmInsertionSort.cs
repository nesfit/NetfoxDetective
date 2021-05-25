using System;
using System.Collections.Generic;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.Models.PmLib.Sorts
{
    internal class PmInsertionSort
    {
        public static List<PmFrameBase> InsertionSort(List<PmFrameBase> list)
        {
            if(list == null) { throw new ArgumentNullException("list"); }

            var count = list.Count;
            for(var j = 1; j < count; j++)
            {
                var key = list[j];
                var i = j - 1;
                for(; i >= 0 && list[i].CompareTo(key) > 0; i--) { list[i + 1] = list[i]; }
                list[i + 1] = key;
            }
            return list;
        }
    }
}