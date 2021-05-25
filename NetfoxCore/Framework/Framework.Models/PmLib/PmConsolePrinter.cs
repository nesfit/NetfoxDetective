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