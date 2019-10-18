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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Netfox.NBARDatabase
{
    public partial class NBAR2Taxonomy
    {
        private NBAR2TaxonomyInfo _nbar2TaxonomyInfo;
        private NBAR2TaxonomyProtocol[] _nbar2TaxonomyProtocol;

        public NBAR2TaxonomyInfo NBAR2TaxonomyInfo
        {
            get { return this._nbar2TaxonomyInfo ?? (this._nbar2TaxonomyInfo = this.Items.FirstOrDefault(item => item is NBAR2TaxonomyInfo) as NBAR2TaxonomyInfo); }
            set { this._nbar2TaxonomyInfo = value; }
        }

        public NBAR2TaxonomyProtocol[] NBAR2TaxonomyProtocol
        {
            get
            {
                return this._nbar2TaxonomyProtocol ?? (this._nbar2TaxonomyProtocol = this.Items.Where(item => item is NBAR2TaxonomyProtocol).Cast<NBAR2TaxonomyProtocol>().ToArray());
            }
            set { this._nbar2TaxonomyProtocol = value; }
        }

        public static NBAR2Taxonomy Nbar2TaxonomyLoader(String taxonomyFilePath = null)
        {
            NBAR2Taxonomy nbar2Taxonomy;
            try
            {
                if(taxonomyFilePath == null)
                {
                    using(var xmlReader = new StringReader(NBAR2Database.DefaultPath))
                    {
                        var serializer = new XmlSerializer(typeof(NBAR2Taxonomy));
                        nbar2Taxonomy = serializer.Deserialize(xmlReader) as NBAR2Taxonomy;
                    }
                }
                else
                {
                    using(var xmlReader = XmlReader.Create(new StreamReader(taxonomyFilePath)))
                    {
                        var serializer = new XmlSerializer(typeof(NBAR2Taxonomy));
                        nbar2Taxonomy = serializer.Deserialize(xmlReader) as NBAR2Taxonomy;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Debugger.Break();
                throw;
            }

            return nbar2Taxonomy;
        }
    }
}