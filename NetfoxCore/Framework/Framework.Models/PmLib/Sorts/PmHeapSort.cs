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
            var j = i * 2 + 1;
            while (j <= m)
            {
                if (j < m)
                {
                    if (list[j].CompareTo(list[j + 1]) < 0)
                    {
                        j = j + 1;
                    }
                }

                if (temp.CompareTo(list[j]) < 0)
                {
                    list[i] = list[j];
                    i = j;
                    j = 2 * i + 1;
                }
                else
                {
                    j = m + 1;
                }
            }

            list[i] = temp;
        }

        public static List<PmFrameBase> HeapSort(List<PmFrameBase> list)
        {
            for (var i = (list.Count - 1) / 2; i >= 0; i--)
            {
                Adjust(list, i, list.Count - 1);
            }

            for (var i = list.Count - 1; i >= 1; i--)
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