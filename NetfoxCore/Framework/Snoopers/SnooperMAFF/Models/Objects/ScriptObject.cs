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
using System.Collections.Generic;
using Netfox.Snoopers.SnooperHTTP.Models;

namespace Netfox.Snoopers.SnooperMAFF.Models.Objects
{
    /// <summary>
    ///  Derived class from Text desribes all script objects.
    /// </summary>
    /// <seealso cref="TextObject" />
    public class ScriptObject : TextObject
    {
        private Jint.Parser.Ast.Program _jsProgram;
        private Jint.Parser.JavaScriptParser _jsParser;
        private Jint.Engine _jsEngine;

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override BaseObject Clone()
        {
            return new ScriptObject(this.OriginalUrl, this.PathToFileName, this.FileName, this.FileExtension, this.ContentType, this.RequestMessage, this.CookieInformation);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptObject"/> class.
        /// </summary>
        /// <param name="sOrigUrl">The original URL.</param>
        /// <param name="sPathToFile">The path to file.</param>
        /// <param name="sFileName">Name of the file.</param>
        /// <param name="sFileExtension">The file extension.</param>
        /// <param name="sContentType">Type of the content.</param>
        /// <param name="oRequestMsg">The request MSG.</param>
        /// <param name="listCookieInformation">The list cookie information.</param>
        public ScriptObject(
            string sOrigUrl,
            string sPathToFile,
            string sFileName,
            string sFileExtension,
            string sContentType,
            HTTPMsg oRequestMsg,
            List<string> listCookieInformation) : base(sOrigUrl, sPathToFile, sFileName, sFileExtension, sContentType, oRequestMsg, listCookieInformation)
        {

            this._jsParser = new Jint.Parser.JavaScriptParser(false);
            this._jsProgram = null;
            //this._jsEngine = new Jint.Engine();
            //this.ParseScript();
        }

        public void ParseScript()
        {
            //const string sSource = @"
            //    var s = 'Hello World From JavaScript ';
            //    write(s);";
            try
            {
                //this._jsProgram = this._jsParser.Parse(sSource);
                ////var oComp = this._jsEngine.Execute(this._jsProgram).GetCompletionValue();
            }
            catch (Exception)
            {
                this._jsProgram = null;
                Console.WriteLine(@"Parsing error in file: " + this.FileName);
            }
        }
    }
}
