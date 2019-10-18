// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka
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
using System.IO;
using System.Linq;
using Castle.Windsor;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperHTTP.Models;
using Netfox.SnooperWebmails.Models;
using Netfox.SnooperWebmails.Models.Analyzers;
using Netfox.SnooperWebmails.Models.Spotters;
using Netfox.SnooperWebmails.Models.SpotterVisitors;
using Netfox.SnooperWebmails.Models.WebmailEvents;

namespace Netfox.SnooperWebmails
{
    public class SnooperWebmails : SnooperBase
    {
        public override string Description => string.Empty;

        public override int[] KnownApplicationPorts => new[]
        {
            80,
            443
        };

        public override SnooperExportBase PrototypExportObject { get; } = new SnooperExportWebmail();

        protected override SnooperExportBase CreateSnooperExport() => new SnooperExportWebmail();

        public override string Name => "Webmails";

        public override string[] ProtocolNBARName => new[]
        {
            "http",
            "ssl"
        };

        private WebmailEventBase CreateEvent(WebmailAnalyzerBase.EventSuggest evt, 
            WebmailAnalyzerBase.WebmailSuggest webmailApp,
            SpotterBase spotter)
        {
            string from;
            string to;
            string cc;
            string bcc;
            string subject;
            string body;

            switch (evt)
            {
                case WebmailAnalyzerBase.EventSuggest.MailDisplayMessage:
                    if (webmailApp == WebmailAnalyzerBase.WebmailSuggest.Seznam)
                    {
                        var o = spotter.Accept(SpotterVisitorsManager.GetSpotterVisitor("Seznam/GetDisplayMessage"));
                        var m = o as MailMsg;
                        if (m != null) return new EventDisplayMessage(this.SnooperExport, m);
                    } 
                    else if (webmailApp == WebmailAnalyzerBase.WebmailSuggest.YahooMailNeo)
                    {
                        var o = spotter.Accept(SpotterVisitorsManager.GetSpotterVisitor("Yahoo/GetDisplayMessage"));
                        var m = o as MailMsg;
                        if (m != null) return new EventDisplayMessage(this.SnooperExport, m);
                    }

                    this.SpotterGetMailFields(spotter, out from, out to, out cc, out bcc, out subject, out body);
                    return new EventDisplayMessage(this.SnooperExport, from, to, cc, bcc, subject, body);
                case WebmailAnalyzerBase.EventSuggest.MailListFolder:
                    if (webmailApp == WebmailAnalyzerBase.WebmailSuggest.YahooMailNeo)
                    {
                        var o = spotter.Accept(SpotterVisitorsManager.GetSpotterVisitor("Yahoo/GetListedMessages"));
                        var l = o as List<MailMsg>;
                        if (l != null) return new EventListFolder(this.SnooperExport, l);
                    }
                    else if (webmailApp == WebmailAnalyzerBase.WebmailSuggest.Seznam)
                    {
                        var o = spotter.Accept(SpotterVisitorsManager.GetSpotterVisitor("Seznam/GetListedMessages"));
                        var l = o as List<MailMsg>;
                        if (l != null) return new EventListFolder(this.SnooperExport, l);
                    }
                    else if (webmailApp == WebmailAnalyzerBase.WebmailSuggest.MicrosoftLive)
                    {
                        // TODO
                        var content = spotter.GetStringContent();
                        var splitted = content.Split(',');
                        var list = new List<MailMsg>();
                        for (int i = 0; i < splitted.Length; i++)
                        {
                            if (!splitted[i].Contains("new HM.Rollup")) continue;
                            var mail = new MailMsg();
                            mail.From = splitted[i + 11];
                            mail.Subject = splitted[i + 27];
                            list.Add(mail);
                        }
                        return new EventListFolder(this.SnooperExport, list);
                    }
                    return new EventListFolder(this.SnooperExport, spotter.GetStringContent());
                default:

                    if (webmailApp == WebmailAnalyzerBase.WebmailSuggest.Seznam)
                    {
                        var o = spotter.Accept(SpotterVisitorsManager.GetSpotterVisitor("Seznam/GetNewMessage"));
                        var m = o as MailMsg;
                        if (m != null) return new EventNewMessage(this.SnooperExport, m);
                    }
                    else if(webmailApp == WebmailAnalyzerBase.WebmailSuggest.MicrosoftLive)
                    {
                        var pars = spotter.GetContentPart("^d$", "*");
                        var value = pars.Substring(1, pars.Length - 2);
                        var parray = value.Split(',');
                        to = parray.Length > 0 ? parray[0] : "";
                        from = parray.Length > 1 ? parray[1] : "";
                        cc = parray.Length > 2 ? parray[2] : "";
                        bcc = parray.Length > 3 ? parray[3] : "";
                        subject = parray.Length > 5 ? parray[5] : "";
                        body = parray.Length > 6 ? parray[6] : "";
                        return new EventNewMessage(this.SnooperExport, from, to, cc, bcc, subject, body);
                    }


                    this.SpotterGetMailFields(spotter, out from, out to, out cc, out bcc, out subject, out body);

                    return new EventNewMessage(this.SnooperExport, from, to, cc, bcc, subject, body);
            }

        }

        private void SpotterGetMailFields(SpotterBase spotter, out string from, out string to, out string cc, out string bcc, out string subject, out string content)
        {
            from = spotter.GetContentPart("^(from|From|FROM|senderEmail)$", "^<?[^@]+@[\\w]+\\.[a-zA-Z]{2,4}>?$");
            to = spotter.GetContentPart("^(to|To|TO|recipientEmail)$", "^<?[^@]+@[\\w]+\\.[a-zA-Z]{2,4}>?$");
            cc = spotter.GetContentPart("^(cc|Cc|CC)$", "^<?[^@]+@[\\w]+\\.[a-zA-Z]{2,4}>?$");
            bcc = spotter.GetContentPart("^(bcc|Bcc|BCC)$", "^<?[^@]+@[\\w]+\\.[a-zA-Z]{2,4}>?$");
            subject = spotter.GetContentPart("^(subject|Subject|SUBJECT)$", ".+");
            content = spotter.GetContentPart("^([Cc]ontent|(simple)?[Bb]ody)$", ".*");
        }

        protected override void ProcessConversation()
        {
            Console.WriteLine("SnooperWebmails.ProcessConversation() called");
            throw new InvalidOperationException();
        }

        protected void ProcessExports()
        {
            Console.WriteLine("SnooperWebmails.ProcessExports() called");

            // create export directory if it doesn't exist
            if (!this.ExportBaseDirectory.Exists)
            {
                this.ExportBaseDirectory.Create();
            }
            
            // start processing and export
            this.OnBeforeDataExporting();

            foreach (var exportObject in this.SourceExports)
            {
                if(exportObject.ExportObjects == null) continue;

                foreach(var exportedObject in exportObject.ExportObjects)
                {
                    var httpObject = exportedObject as SnooperExportedDataObjectHTTP;
                    if(httpObject != null && httpObject.Message.MessageType == MessageType.REQUEST)
                    {

                        var header = httpObject.Message.HTTPHeader as HttpRequestHeader;

                        var analyzer = WebmailAnalyzerFactory.GetAnalyzer(header);

                        var detectedEvents = new List<WebmailAnalyzerBase.EventSuggest>();
                        SpotterBase spotter;

                        if (analyzer.IsMsgWebmailEvent(httpObject.Message, ref detectedEvents, out spotter))
                        {
                            WebmailEventBase webmailEvent;

                            // TODO should not be set with new message
                            if(detectedEvents.Any(x => x.Equals(WebmailAnalyzerBase.EventSuggest.MailListFolder) 
                                                            || x.Equals(WebmailAnalyzerBase.EventSuggest.MailDisplayMessage)))
                            {

                                // response might need another type of spotter
                                SpotterFactory.Pool.ReturningSpotter(spotter);

                                if (httpObject.Message.PairMessages.FirstOrDefault()?.HTTPHeader.Fields.ContainsKey("Content-Type") ?? false)
                                {
                                    spotter = SpotterFactory.Pool.GetSpotterOrWait(httpObject.Message.PairMessages.FirstOrDefault()?.HTTPHeader.Fields["Content-Type"].First());
                                    spotter.Init(httpObject.Message.PairMessages.FirstOrDefault());
                                }
                                // if it is list event or display event all the stuff is in response

                                // check for false positives
                                if(!spotter.IsSpottable())
                                {
                                    SpotterFactory.Pool.ReturningSpotter(spotter);
                                    continue;
                                }

                            }

                            foreach(var eventSuggest in detectedEvents)
                            {
                                webmailEvent = this.CreateEvent(eventSuggest,analyzer.WebmailApp,spotter);
                                webmailEvent.TimeStamp = httpObject.TimeStamp;
                                webmailEvent.ExportSources.AddRange(httpObject.Message.ExportSources);
                                this.SnooperExport.AddExportObject(webmailEvent);
                            }

                        }

                        // return spotter to the pool
                        if (spotter != null) SpotterFactory.Pool.ReturningSpotter(spotter);

                    }
                }
            }

            this.OnAfterDataExporting();

        }

        protected override void RunBody()
        {
            if (this.SelectedConversations != null)
            {
                base.ProcessAssignedConversations();
            }
            else
            {
                this.OnConversationProcessingBegin();
                this.ProcessExports();
                this.OnConversationProcessingEnd();
            }

        }

        public SnooperWebmails() { }
        
        public SnooperWebmails(WindsorContainer investigationWindsorContainer, IEnumerable<SnooperExportBase> sourceExports, DirectoryInfo exportDirectory) : base(investigationWindsorContainer, sourceExports, exportDirectory) {}
    }
}
