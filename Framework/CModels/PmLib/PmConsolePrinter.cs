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

namespace Netfox.Framework.Models.PmLib
{
    /// <summary>
    ///     Auxiliary class for color differentiated console output
    /// </summary>
    public static class PmConsolePrinter
    {
        private static Boolean _enable = false;

        /// <summary>
        ///     Produces yellow colored output to Console, purely estetic reasons.
        /// </summary>
        /// <param name="text">Input text string</param>
        public static void PrintDebug(String text)
        {
            if(!_enable) { return; }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void PrintDebugDelimiter()
        {
            if(!_enable) { return; }
            PrintDebug("-------------------------------------");
        }

        /// <summary>
        ///     Produces red colored output to Console, purely estetic reasons.
        /// </summary>
        /// <param name="text">Input text string</param>
        public static void PrintError(String text)
        {
            if(!_enable) { return; }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        /// <summary>
        ///     Produces standart color output with NO line break
        /// </summary>
        /// <param name="text">Input text string</param>
        /// <param name="args"> </param>
        public static void PrintInfo(this String text, params Object[] args)
        {
            if(!_enable) { return; }
            Console.Write(text, args);
        }

        /// <summary>
        ///     Produces standart color output
        /// </summary>
        /// <param name="text">Input text string</param>
        /// <param name="args"> </param>
        public static void PrintInfoEol(this String text, params Object[] args)
        {
            if(!_enable) { return; }
            Console.WriteLine(text, args);
        }
    }
}