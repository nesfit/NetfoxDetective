/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2013 Brno University of Technology - Faculty of Information Technology (http://www.fit.vutbr.cz)
 * Author(s):
 * Vladimir Vesely (mailto:ivesely@fit.vutbr.cz)
 * Martin Mares (mailto:xmares04@stud.fit.vutbr.cz)
 * Jan Plusal (mailto:xplusk03@stud.fit.vutbr.cz)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
 * documentation files (the "Software"), to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.Models.PmLib.Sorts
{
    /// <summary>
    ///     Inplace unstable sorting algorithm
    /// </summary>
    internal class PmHeapSort
    {
        public static void Adjust(List<PmFrameBase> list, Int32 i, Int32 m)
        {
            var temp = list[i];
            var j = i*2 + 1;
            while(j <= m)
            {
                if(j < m) { if(list[j].CompareTo(list[j + 1]) < 0) { j = j + 1; } }

                if(temp.CompareTo(list[j]) < 0)
                {
                    list[i] = list[j];
                    i = j;
                    j = 2*i + 1;
                }
                else
                { j = m + 1; }
            }
            list[i] = temp;
        }

        public static List<PmFrameBase> HeapSort(List<PmFrameBase> list)
        {
            for(var i = (list.Count - 1)/2; i >= 0; i--) { Adjust(list, i, list.Count - 1); }

            for(var i = list.Count - 1; i >= 1; i--)
            {
                var temp = list[0];
                list[0] = list[i];
                list[i] = temp;
                Adjust(list, 0, i - 1);
            }

            return list;
        }
    }
}