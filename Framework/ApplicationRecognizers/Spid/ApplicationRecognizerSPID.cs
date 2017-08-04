using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Services;
using Netfox.NBARDatabase;
using Netfox.NBARDatabase.Properties;
using ProtocolIdentification;

namespace Spid
{
    /// <summary> An application protocol recognizer spid.</summary>
    public class ApplicationRecognizerSpid : ApplicationRecognizerBase
    {
        /// <summary> The configuration.</summary>
        private readonly Configuration _config;

        /// <summary> The protocol models.</summary>
        private readonly SortedList<String, ProtocolModel> _protocolModels;

        /// <summary> The model extractor.</summary>
        private SessionAndProtocolModelExtractor _modelExtractor;
        

        /// <summary> Default constructor.</summary>
        public ApplicationRecognizerSpid(NBARProtocolPortDatabase nbarProtocolPortDatabase, String configFilePath = null, String protocolDatabaseFilePath = null, Boolean prototype = false):base(nbarProtocolPortDatabase)
        {
            if(prototype) { return; }
            if(configFilePath == null)
            {
                if(File.Exists(NBARprotocols.Default.SPID_config)) {
                    configFilePath = NBARprotocols.Default.SPID_config;
                }
                else
                {
                    throw new ArgumentException(@"Invalid argument and default configs do not exists.", "configFilePath");
                }
            }
            if(protocolDatabaseFilePath == null)
            {
                if(File.Exists(NBARprotocols.Default.DefaultProtocolModelDatabase)) {
                    protocolDatabaseFilePath = NBARprotocols.Default.DefaultProtocolModelDatabase;
                }
                else
                {
                    throw new ArgumentException(@"Invalid argument and default configs do not exists.", "protocolDatabaseFilePath");
                }
            }

            var configs = Configuration.GetInstances(configFilePath);
            this._config = configs.First();

            this._protocolModels = new SortedList<string, ProtocolModel>();
            //10 000 sessions á 30kB => 300 MB
            //this.sessionHandler = new SessionHandler(10000);
            //this._protocolModels = new SortedList<String, ProtocolModel>();
            //try
            //{
            //    this.openProtocolModelDatabaseFile(protocolDatabaseFilePath, false);
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine("Unable to load protocol database " + protocolDatabaseFilePath + ".\n" + e.Message, "Database Not Loaded");
            //}
        }

        public override String Name => @"SPID";

        public override String Description => @"Using DPI combined with statistics to find a best matching protocol model.";

        public override UInt32 Priority => 4;

        public override String Type => "DPI, statistics combined.";

        /// <summary> Gets the model extractor.</summary>
        /// <value> The model extractor.</value>
        private SessionAndProtocolModelExtractor ModelExtractor
            => this._modelExtractor ?? (this._modelExtractor = new SessionAndProtocolModelExtractor(this._protocolModels, this._config));

        public String RecognizeConversation2(L7Conversation conversation)
        {
            var appTag = this.ModelExtractor.RunRecognition(conversation);
            if(appTag == null) { return null; }
            return appTag;
        }

        public void UpdateModelForProtocol(L7Conversation conversation)
        {
            //TODO implement
            var protModel = this.ModelExtractor.GetProtocolModelFromConversation(conversation);

            if(protModel == null || conversation.AppTag== null) { return; }

            protModel.ProtocolName = conversation.AppTag;
            if(this._protocolModels.ContainsKey(protModel.ProtocolName)) {
                this._protocolModels[protModel.ProtocolName] = this._protocolModels[protModel.ProtocolName].MergeWith(protModel);
            }
            else
            {
                this._protocolModels.Add(protModel.ProtocolName, protModel);
            }
        }

        /// <summary>
        ///     This method is called after all conversations are tracked. You can access to metadata about
        ///     conversations providing basic information about BidirectionalFlow or if need be you may
        ///     access frames. RealFrames are stored only as frame numbers which serves as their identifier
        ///     in PCAP. To access frames use IBtBidirectionalFlowValue`s GetFrames() or
        ///     Get{Up|Down}FlowsFrames()
        ///     You can also access to raw reassembled and IPv4 defragmented data stream using
        ///     IBtBidirectionalFlowValue`s PreparePDUs()
        ///     and then you can access L7PDUs, {up|down}FlowPDUs, but pleas keep in mind that PreparePDUs()
        ///     is little bit time consuming so if you do not need it, do not use it.
        /// </summary>
        /// <param name="conversation"> The conversation. </param>
        public override IReadOnlyList<NBAR2TaxonomyProtocol> RecognizeConversation(L7Conversation conversation)
        {
            var appTag = this.ModelExtractor.RunRecognition(conversation);
            if(appTag == null) { return new List<NBAR2TaxonomyProtocol>(); }
            return new List<NBAR2TaxonomyProtocol>
            {
                this.NBARProtocolPortDatabase.GetNbar2TaxonomyProtocol(appTag)
            };
        }

        /// <summary> Gets the protocol models from database files in this collection.</summary>
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// <param name="filename"> Filename of the file. </param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the protocol models from database
        ///     files in this collection.
        /// </returns>
        private IEnumerable<ProtocolModel> GetProtocolModelsFromDatabaseFile(String filename)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filename);
            if(xmlDoc.DocumentElement.SelectSingleNode("/protocolModels").Attributes["fingerprintLength"].Value
               != AttributeFingerprintHandler.Fingerprint.FINGERPRINT_LENGTH.ToString()) {
                   throw new Exception("Fingerprint length is not correct!");
               }
            foreach(XmlNode protocolModelNode in xmlDoc.DocumentElement.SelectNodes("/protocolModels/protocolModel"))
            {
                var protocolName = protocolModelNode.SelectSingleNode("@name").Value;
                var sessionCount = Int32.Parse(protocolModelNode.SelectSingleNode("@sessionCount").Value);
                var observationCount = UInt64.Parse(protocolModelNode.SelectSingleNode("@observationCount").Value);
                //ProtocolIdentification.ProtocolModel model=new ProtocolIdentification.ProtocolModel(name);
                var attributeFingerprintHandlers = new SortedList<String, AttributeFingerprintHandler>();
                var defaultPorts = (from XmlNode defaultPortNode in protocolModelNode.SelectNodes("defaultPorts/port")
                    select UInt16.Parse(defaultPortNode.InnerText)).ToList();

                foreach(XmlNode attributeFingerprintNode in protocolModelNode.SelectNodes("attributeFingerprint"))
                {
                    var attributeMeterName = attributeFingerprintNode.SelectSingleNode("@attributeMeterName").Value;
                    var measurementCount = UInt64.Parse(attributeFingerprintNode.SelectSingleNode("@measurementCount").Value);

                    var probabilityNodeList = attributeFingerprintNode.SelectNodes("bin");
                    var probabilityDistributionVector = new Double[probabilityNodeList.Count];

                    for(var i = 0; i < probabilityDistributionVector.Length; i++) {
                        probabilityDistributionVector[i] = Double.Parse(probabilityNodeList[i].InnerText, CultureInfo.InvariantCulture.NumberFormat);
                    }
                    //.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
                    attributeFingerprintHandlers.Add(attributeMeterName, new AttributeFingerprintHandler(attributeMeterName, probabilityDistributionVector, measurementCount));
                }

                yield return new ProtocolModel(protocolName, attributeFingerprintHandlers, sessionCount, observationCount, defaultPorts, this._config.ActiveAttributeMeters);
            }
        }

        /// <summary> Opens protocol model database file.</summary>
        /// <param name="filename">                 Filename of the file. </param>
        /// <param name="appendToExistingDatabase"> true to append to existing database. </param>
        private void openProtocolModelDatabaseFile(String filename, Boolean appendToExistingDatabase)
        {
            if(!appendToExistingDatabase) { this._protocolModels.Clear(); }
            foreach(var model in this.GetProtocolModelsFromDatabaseFile(filename))
            {
                if(this._protocolModels.ContainsKey(model.ProtocolName)) {
                    this._protocolModels[model.ProtocolName] = this._protocolModels[model.ProtocolName].MergeWith(model);
                }
                else
                {
                    this._protocolModels.Add(model.ProtocolName, model);
                }
            }
        }
    }
}