using System;
using System.Collections.Generic;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.Models.PmLib.Sorts
{
    /// <summary>
    ///     Inplace unstable sorting algorithm
    /// </summary>
    internal class PmMergeSort
    {
        public static void Merge(IList<PmFrameBase> list, Int32 offset, Int32 sizei, Int32 sizej)
        {
            PmFrameBase temp;
            var ii = 0;
            var ji = sizei;
            var flength = sizei + sizej;

            for (var f = 0; f < (flength - 1); f++)
            {
                if (sizei == 0 || sizej == 0)
                {
                    break;
                }

                if (list[offset + ii].CompareTo(list[offset + ji]) < 0)
                {
                    ii++;
                    sizei--;
                }
                else
                {
                    temp = list[offset + ji];

                    for (var z = (ji - 1); z >= ii; z--)
                    {
                        list[offset + z + 1] = list[offset + z];
                    }

                    ii++;

                    list[offset + f] = temp;

                    ji++;
                    sizej--;
                }
            }
        }

        public static List<PmFrameBase> MergeSort(List<PmFrameBase> list, Int32 offset = 0, Int32 len = -1)
        {
            if (list == null)
            {
                return null;
            }

            if (len == -1)
            {
                len = list.Count;
            }

            Int32 listsize;

            for (listsize = 1; listsize <= len; listsize *= 2)
            {
                for (Int32 i = 0, j = listsize; (j + listsize) <= len; i += (listsize * 2), j += (listsize * 2))
                {
                    Merge(list, i, listsize, listsize);
                }
            }

            listsize /= 2;

            var xsize = len % listsize;
            if (xsize > 1)
            {
                MergeSort(list, len - xsize, xsize);
            }

            Merge(list, 0, listsize, xsize);

            return list;
        }
    }
}