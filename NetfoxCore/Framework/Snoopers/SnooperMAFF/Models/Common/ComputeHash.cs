// Copyright (c) 2017 Jan Pluskal, Vit Janecek
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
using System.Security.Cryptography;
using System.Text;

namespace Netfox.Snoopers.SnooperMAFF.Models.Common
{
    /// <summary>
    /// Compute file hash from his content
    /// </summary>
    static class ComputeHash
    {
        private const int MaxHashSize = 4096;
        /// <summary>
        /// Gets the MD5 hash from string (overloaded).
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>Return MD5 hash in string form</returns>
        static public string GetMd5Hash(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var hashed = md5.ComputeHash(Encoding.Unicode.GetBytes(str));

            var sb = new StringBuilder();
            foreach (var t in hashed) { sb.Append(t.ToString("x2")); }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the MD5 hash from byyte array content (overloaded).
        /// </summary>
        /// <param name="array">The byte array.</param>
        /// <returns>Return MD5 hash in string form</returns>
        static public string GetMd5Hash(byte[] array)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] byteOurHash;
            if (array.LongLength <= MaxHashSize)
            {
                byteOurHash = md5.ComputeHash(array);
            }
            else
            {
                var byteTempArray = new byte[MaxHashSize];
                Buffer.BlockCopy(array, 0, byteTempArray, 0, MaxHashSize);
                byteOurHash = md5.ComputeHash(array);
            }

            var sb = new StringBuilder();
            foreach (var t in byteOurHash)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }

    }
}
