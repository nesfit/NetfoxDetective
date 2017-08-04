using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Xml;
using ProtocolIdentification;

namespace Spid
{
    //[DefaultPropertyAttribute("DivergenceThreshold")]
    public class Configuration
    {
        public enum DateTimeFormat
        {
            CURRENT_UI_CULTURE,
            INVARIANT_CULTURE,
            UNIX_TIME
        }

        private static Configuration singletonInstance;
        private String configXmlFile;

        private Configuration(String configXmlFile, ICollection<String> activatedOptionalAttributeMeterNames)
        {
            this.configXmlFile = configXmlFile;

            //read the config file
            this.Load(activatedOptionalAttributeMeterNames);
        }

        [Description(
            "A high value might generate false positives and a low value might generate false negatives. A value of 2.2 or 2.3 is appropriate when all attributeMeters are used. ")]
        public Double DivergenceThreshold { get; set; }

        [Description("More than 1000 sessions can cause your RAM to fill up")]
        public Int32 MaxSimultaneousSessions { get; set; }

        [Description("There is no point in setting a value higher than 100 since the models have only been trained on the first 100 packets of various sessions")]
        public Int32 MaxFramesToInspectPerSession { get; set; }

        [Description("Whether or not to display the session details with divergence measurements")]
        public Boolean DisplayAllProtocolModelDivergences { get; set; }

        [Description("The format to use for timestamps")]
        public DateTimeFormat TimestampFormat { get; set; }

        [Description("Whether or not to display the .txt log file after parsing a pcap file")]
        public Boolean DisplayLogFile { get; set; }

        [Description("Whether or not protocol models for unidirectional flows should be created when reading training data (only works for TCP sessions)")]
        public Boolean AppendUnidirectionalSessions { get; set; }

        [Browsable(false)]
        public ICollection<String> ActiveAttributeMeters { get; private set; }

        //[DisplayName("Attribute Meter Settings")]

        [Description("Disabling attribute meters will improve speed and free more RAM")]
        public ReadOnlyCollection<AttributeMeterSetting> AttributeMeterSettings { get; private set; }

        public String FormatDateTime(DateTime timestamp)
        {
            if(this.TimestampFormat == DateTimeFormat.INVARIANT_CULTURE) { return timestamp.ToString(CultureInfo.InvariantCulture.DateTimeFormat); }
            if(this.TimestampFormat == DateTimeFormat.UNIX_TIME)
            {
                //from PcapFileHandler.PcapFileWriter.cs in NetworkMiner
                var referenceTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var delta = timestamp.ToUniversalTime().Subtract(referenceTime);
                //The smallest unit of time is the tick, which is equal to 100 nanoseconds.
                var totalMicroseconds = delta.Ticks/10;
                var seconds = (UInt32) (totalMicroseconds/1000000);
                var microseconds = (UInt32) (totalMicroseconds%1000000);
                return seconds.ToString(CultureInfo.InvariantCulture.NumberFormat) + "." + microseconds.ToString("D6", CultureInfo.InvariantCulture.NumberFormat) + "000";
            }
            return timestamp.ToString(CultureInfo.CurrentUICulture.DateTimeFormat);
        }

        public static Configuration GetInstance() => singletonInstance;
        /*
        public static void CreateInstance(string configXmlFile) {
            singletonInstance=new Configuration(configXmlFile);
        }*/

        public static IEnumerable<Configuration> GetInstances(String configXmlFile)
        {
            var options = GetOptionalAttributeMeterNames(configXmlFile);
            var activatedOptions = new List<String>();
            if(options.Length == 0)
            {
                singletonInstance = new Configuration(configXmlFile, activatedOptions);
                yield return singletonInstance;
            }
            else
            {
                foreach(var option in options)
                {
                    activatedOptions.Clear();
                    activatedOptions.Add(option);
                    singletonInstance = new Configuration(configXmlFile, activatedOptions);
                    yield return singletonInstance;
                }
            }
        }

        public void Load() => this.Load(new List<String>());

        public void Load(ICollection<String> activatedOptionalAttributeMeterNames)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(this.configXmlFile);

            var divergenceNode = xmlDoc.DocumentElement.SelectSingleNode("/config/divergenceThreshold");
            this.DivergenceThreshold = Double.Parse(divergenceNode.InnerText, CultureInfo.InvariantCulture.NumberFormat);

            var maxNode = xmlDoc.DocumentElement.SelectSingleNode("/config/maxSimultaneousSessions");
            this.MaxSimultaneousSessions = Int32.Parse(maxNode.InnerText, CultureInfo.InvariantCulture.NumberFormat);

            var framesCountNode = xmlDoc.DocumentElement.SelectSingleNode("/config/maxFramesToInspectPerSession");
            this.MaxFramesToInspectPerSession = Int32.Parse(framesCountNode.InnerText, CultureInfo.InvariantCulture.NumberFormat);

            var displayAllNode = xmlDoc.DocumentElement.SelectSingleNode("/config/displayAllProtocolModelDivergences");
            this.DisplayAllProtocolModelDivergences = displayAllNode.InnerText.Trim() == "true";

            var timestampFormatNode = xmlDoc.DocumentElement.SelectSingleNode("/config/timestampFormat");
            this.TimestampFormat = (DateTimeFormat) Int32.Parse(timestampFormatNode.InnerText, CultureInfo.InvariantCulture.NumberFormat);

            var displayLogFileNode = xmlDoc.DocumentElement.SelectSingleNode("/config/displayLogFile");
            this.DisplayLogFile = displayLogFileNode.InnerText.Trim() == "true";

            var unidirNode = xmlDoc.DocumentElement.SelectSingleNode("/config/appendUnidirectionalSessions");
            this.AppendUnidirectionalSessions = unidirNode.InnerText.Trim() == "true";


            var attributeMetersNode = xmlDoc.DocumentElement.SelectSingleNode("/config/attributeMeters");
            var tmpAttributeMeterSettings = new List<AttributeMeterSetting>();
            var attributeMeterNodes = attributeMetersNode.SelectNodes("attributeMeter");
            for(var i = 0; i < attributeMeterNodes.Count; i++)
            {
                var s = new AttributeMeterSetting(attributeMeterNodes[i].SelectSingleNode("@attributeName").Value);
                if(attributeMeterNodes[i].SelectSingleNode("@active").Value.Equals("true", StringComparison.InvariantCultureIgnoreCase)) {
                    s.Active = true;
                }
                else if(activatedOptionalAttributeMeterNames.Contains(s.Name)
                        && attributeMeterNodes[i].SelectSingleNode("@active").Value.Equals("optional", StringComparison.InvariantCultureIgnoreCase)) {
                            s.Active = true;
                        }
                else
                {
                    s.Active = false;
                }
                tmpAttributeMeterSettings.Add(s);
            }
            /*
            foreach(System.Xml.XmlNode attributeMeterNode in attributeMetersNode.SelectNodes("attributeMeter")) {
                AttributeMeterSetting s=new AttributeMeterSetting(attributeMeterNode.SelectSingleNode("@attributeName").Value);
                if(attributeMeterNode.SelectSingleNode("@active").Value=="true")
                    s.Active=true;
                else
                    s.Active=false;
                tmpAttributeMeterSettings.Add(s);
            }
             * */
            this.AttributeMeterSettings = new ReadOnlyCollection<AttributeMeterSetting>(tmpAttributeMeterSettings);

            //make a short test to ensure that the config.xml holds all the protocols
            var pTest = new ProtocolModel("test");
            if(pTest.AttributeFingerprintHandlers.Count != attributeMetersNode.SelectNodes("attributeMeter").Count)
            {
                throw new Exception("The number of fingerprints in config.xml is not correct!\nXML value=" + attributeMetersNode.SelectNodes("attributeName").Count + ", should be "
                                    + pTest.AttributeFingerprintHandlers.Count);
            }
            foreach(XmlNode n in attributeMetersNode.SelectNodes("attributeMeter"))
            {
                if(!pTest.AttributeFingerprintHandlers.Keys.Contains(n.SelectSingleNode("@attributeName").Value)) {
                    throw new Exception("AttributeMeter " + n.SelectSingleNode("@attributeName").Value + " does not exist (only in config.xml)");
                }
            }

            this.ActiveAttributeMeters = new List<String>();
            foreach(var s in this.AttributeMeterSettings) { if(s.Active) { this.ActiveAttributeMeters.Add(s.Name); } }
        }

        public void Save()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(this.configXmlFile);

            var divergenceNode = xmlDoc.DocumentElement.SelectSingleNode("/config/divergenceThreshold");
            divergenceNode.InnerText = this.DivergenceThreshold.ToString(CultureInfo.InvariantCulture.NumberFormat);

            var maxNode = xmlDoc.DocumentElement.SelectSingleNode("/config/maxSimultaneousSessions");
            maxNode.InnerText = this.MaxSimultaneousSessions.ToString(CultureInfo.InvariantCulture.NumberFormat);

            var framesCountNode = xmlDoc.DocumentElement.SelectSingleNode("/config/maxFramesToInspectPerSession");
            framesCountNode.InnerText = this.MaxFramesToInspectPerSession.ToString(CultureInfo.InvariantCulture.NumberFormat);

            var displayAllNode = xmlDoc.DocumentElement.SelectSingleNode("/config/displayAllProtocolModelDivergences");
            if(this.DisplayAllProtocolModelDivergences) {
                displayAllNode.InnerText = "true";
            }
            else
            {
                displayAllNode.InnerText = "false";
            }

            var timestampFormatNode = xmlDoc.DocumentElement.SelectSingleNode("/config/timestampFormat");
            timestampFormatNode.InnerText = ((Int32) this.TimestampFormat).ToString();

            var displayLogFileNode = xmlDoc.DocumentElement.SelectSingleNode("/config/displayLogFile");
            if(this.DisplayLogFile) {
                displayLogFileNode.InnerText = "true";
            }
            else
            {
                displayLogFileNode.InnerText = "false";
            }

            var unidirNode = xmlDoc.DocumentElement.SelectSingleNode("/config/appendUnidirectionalSessions");
            if(this.AppendUnidirectionalSessions) {
                unidirNode.InnerText = "true";
            }
            else
            {
                unidirNode.InnerText = "false";
            }

            //System.Xml.XmlNode attributeMetersNode=xmlDoc.DocumentElement.SelectSingleNode("/config/attributeMeters");
            foreach(var s in this.AttributeMeterSettings)
            {
                var attributeMeterNode = xmlDoc.DocumentElement.SelectSingleNode("/config/attributeMeters/attributeMeter[@attributeName='" + s.Name + "']");
                if(s.Active) {
                    attributeMeterNode.SelectSingleNode("@active").Value = "true";
                }
                else
                {
                    attributeMeterNode.SelectSingleNode("@active").Value = "false";
                }
            }

            xmlDoc.Save(this.configXmlFile);
        }

        public new String ToString()
        {
            var sb = new StringBuilder();
            sb.Append("# config/divergenceThreshold: " + this.DivergenceThreshold.ToString(CultureInfo.InvariantCulture.NumberFormat) + "\r\n");
            sb.Append("# config/maxSimultaneousSessions: " + this.MaxSimultaneousSessions.ToString(CultureInfo.InvariantCulture.NumberFormat) + "\r\n");
            sb.Append("# config/maxFramesToInspectPerSession: " + this.MaxFramesToInspectPerSession.ToString(CultureInfo.InvariantCulture.NumberFormat) + "\r\n");
            sb.Append("# config/displayAllProtocolModelDivergences: ");
            if(this.DisplayAllProtocolModelDivergences) {
                sb.Append("true\r\n");
            }
            else
            {
                sb.Append("false\r\n");
            }
            sb.Append("# config/timestampFormat: " + this.TimestampFormat + "\r\n");
            sb.Append("# config/displayLogFile: ");
            if(this.DisplayLogFile) {
                sb.Append("true\r\n");
            }
            else
            {
                sb.Append("false\r\n");
            }

            sb.Append("# config/appendUnidirectionalSessions: ");
            if(this.AppendUnidirectionalSessions) {
                sb.Append("true\r\n");
            }
            else
            {
                sb.Append("false\r\n");
            }

            foreach(var s in this.AttributeMeterSettings)
            {
                sb.Append("# config/attributeMeters/attributeMeter/" + s.Name + ": ");
                if(s.Active) {
                    sb.Append("active\r\n");
                }
                else
                {
                    sb.Append("-\r\n");
                }
            }
            return sb.ToString();
        }

        private static String[] GetOptionalAttributeMeterNames(String configXmlFile)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(configXmlFile);
            var attributeMetersNode = xmlDoc.DocumentElement.SelectSingleNode("/config/attributeMeters");
            var tmpAttributeMeterSettings = new List<AttributeMeterSetting>();
            var optionalAttributeMeterNodes = attributeMetersNode.SelectNodes("attributeMeter[@active='optional']");
            var returnArray = new String[optionalAttributeMeterNodes.Count];
            for(var i = 0; i < returnArray.Length; i++) { returnArray[i] = optionalAttributeMeterNodes[i].SelectSingleNode("@attributeName").Value; }
            return returnArray;
        }
    }
}