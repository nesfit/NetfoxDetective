// Copyright (c) 2017 Jan Pluskal, Filip Karpisek
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
using System.Text;
using Castle.Windsor;
using Netfox.Core.Enums;
using Netfox.Core.Models.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Enums;
using Netfox.Framework.Models.Snoopers;
using Netfox.Snoopers.SnooperSIP.Enums;
using Netfox.Snoopers.SnooperSIP.Models;
using Netfox.Snoopers.SnooperSIP.Models.Message;

namespace Netfox.Snoopers.SnooperSIP
{
    public class SnooperSIP : SnooperBase
	{
		private Dictionary<string, SIPEvent> _eventsDictionary;
        public SnooperSIP() { } //Must exists for creation of snooper prototype to obtain describing information
        public SnooperSIP(WindsorContainer investigationWindsorContainer, SelectedConversations conversations, DirectoryInfo exportDirectory, bool ignoreApplicationTags)
            : base(investigationWindsorContainer, conversations, exportDirectory, ignoreApplicationTags)
        { this._eventsDictionary = new Dictionary<string, SIPEvent>(); }

		public override string Name => "SIP";

		// here we say what type of conversations (by NBAR name) we want to process
		public override string[] ProtocolNBARName => new[] { "sip" };

		public override string Description => "Snooper for SIP protocol";

		public override int[] KnownApplicationPorts => new[] { 5060 };

	    internal new SnooperExportSIP SnooperExport => base.SnooperExport as SnooperExportSIP;
        public override SnooperExportBase PrototypeExportObject { get; } = new SnooperExportSIP();

        protected override SnooperExportBase CreateSnooperExport() => new SnooperExportSIP();

        protected override void RunBody()
		{
            base.ProcessAssignedConversations();
		}

	    private bool PossibleCall(SIPMsg message)
	    {
		    switch(message.Type) {
				case SIPMsg.SIPMsgType.Request:
				    var method = message.RequestLine.Method.ToUpper();
				    if(SIPEvent.CallMethods.Contains(method)) return true;
				    break;
				case SIPMsg.SIPMsgType.Status:
				    var status = message.StatusLine.StatusInfo.ToLower();
					if (SIPEvent.CallStatuses.Contains(status)) return true;
					break;
		    }
		    return false;
	    }

	    private bool PossibleAuthentization(SIPMsg message)
	    {

		    switch(message.Type)
		    {
			    case SIPMsg.SIPMsgType.Request:
				    var method = message.RequestLine.Method.ToUpper();
				    if(SIPEvent.AuthentizationMethods.Contains(method)) return true;
				    break;
		    }
		    return false;
	    }

		protected override void ProcessConversation()
	    {
		    {
                var stream = new PDUStreamBasedProvider(this.CurrentConversation, EfcPDUProviderType.SingleMessage);
			    var reader = new PDUStreamReader(stream, Encoding.UTF8);
			    do
			    {
				    this.OnBeforeProtocolParsing();
				    //Parse protocol....
				    SIPMsg _message;
				    _message = new SIPMsg(reader);

				    if(!_message.Valid)
                    {
                        this.SnooperExport.TimeStampFirst = _message.Timestamp;
                        this.SnooperExport.AddExportReport(
                            ExportReport.ReportLevel.Warn,
                            this.Name,
                            "parsing of SIP message failed, frame numbers: " + string.Join(",", _message.Frames) + ": " +
						    _message.InvalidReason,
                            _message.ExportSources);
					    Console.WriteLine("parsing of SIP message failed, frame numbers: " + string.Join(",", _message.Frames) + ": "+ _message.InvalidReason);
					    continue;
				    }
				    this.OnAfterProtocolParsing();
				    //Console.WriteLine("successful parsing: frame " + string.Join(",", _message.FrameNumbers.ToArray()));
				    //Do some magic...
				    SIPEvent _event;
				    if(this._eventsDictionary.TryGetValue(_message.Headers.CallID, out _event))
				    {
					    //Event already present
					    switch(_event.Type)
					    {
						    case SIPEventType.Authentization:
							    //Console.WriteLine("authentication "+_message.Headers.CallID+" present");
							    _event.Update(_message);
							    break;
						    case SIPEventType.Call:
							    //Console.WriteLine("call "+_message.Headers.CallID+" present");
							    _event.Update(_message);
							    break;
							case SIPEventType.Unknown:
							    var oldEvent = _event as SIPUnknownEvent;
							    if(this.PossibleCall(_message))
							    {
									this._eventsDictionary.Remove(_message.Headers.CallID);
									this.SnooperExport.DiscardExportObject(_event);
								    _event = new SIPCall(this.SnooperExport);
									this._eventsDictionary.Add(_message.Headers.CallID, _event);
                                    _event.UpdateFromUnkownEvent(oldEvent);
                                }
								else if(this.PossibleAuthentization(_message))
								{
									this._eventsDictionary.Remove(_message.Headers.CallID);
									this.SnooperExport.DiscardExportObject(_event);
									_event = new SIPAuthentization(this.SnooperExport);
									this._eventsDictionary.Add(_message.Headers.CallID, _event);
                                    _event.UpdateFromUnkownEvent(oldEvent);
                                }
								_event.Update(_message);
								break;
						    default:
							    Console.WriteLine("unknown event " + _message.Headers.CallID + " present");
							    //TODO throw some exception
							    break;
					    }
				    }
				    else
				    {
					    //New event, create
					    if(_message.Type == SIPMsg.SIPMsgType.Request && _message.RequestLine.Method == "REGISTER")
						{
							_event = new SIPAuthentization(this.SnooperExport);
						    this._eventsDictionary.Add(_message.Headers.CallID, _event);

						    //Console.WriteLine("authentication " + _message.Headers.CallID + " added");
						    _event.Update(_message);
						}
					    else if(_message.Type == SIPMsg.SIPMsgType.Request && _message.RequestLine.Method == "INVITE")
					    {
						    _event = new SIPCall(this.SnooperExport);
						    this._eventsDictionary.Add(_message.Headers.CallID, _event);

						    //Console.WriteLine("call " + _message.Headers.CallID + " added");
						    _event.Update(_message);
					    }
					    else // type can't be easily decided
					    {
						    if(this.PossibleCall(_message)) { _event = new SIPCall(this.SnooperExport); }
							else if(this.PossibleAuthentization(_message)) { _event = new SIPAuthentization(this.SnooperExport); }
							else { _event = new SIPUnknownEvent(this.SnooperExport); }
						    this._eventsDictionary.Add(_message.Headers.CallID, _event);
							_event.Update(_message);
						}
				    }
				    //        this.eventExporter.AddExportedData(); //Export some meaningful message, object, what so ever...
				    //        this.eventExporter.AddExportReport(); //Export problem, exception some meaningful note that will be part of exported data object or export report in case that no data were exported between two escalation of BeforeProtocolParsing 
				    //Console.WriteLine(_message.ToString());
			    } while(reader.NewMessage());

			    //Export
				this.OnBeforeDataExporting();
			    var _callCounter = 0;
			    foreach(var kvp in this._eventsDictionary)
			    {
				    if(kvp.Value.Type == SIPEventType.Unknown)
				    {
					    this.SnooperExport.DiscardExportObject(kvp.Value);
				    }
					else
					{
						if (kvp.Value is SIPCall)
						{
							// Process RTP flows of every call
							var s = kvp.Value as SIPCall;
							//s.CallId = _callCounter.ToString();
							// Pass it inside the call
							//s.SetExportedPayloads(this.ProcessRTP(s.RTPAddresses, _callCounter.ToString()));
							++_callCounter;
						}

						//Console.WriteLine("event " + kvp.Key);
						//Console.WriteLine(kvp.Value.ToString());

						kvp.Value.ExportValidity = ExportValidity.ValidWhole;

						//TODO there should be list of guids (frames, other objects)
						//kvp.Value.ExportSources.Add(this.CurrentConversation); //todo switch to used PDUs
						this.SnooperExport.AddExportObject(kvp.Value);
					}
				}
				this.OnAfterDataExporting();
			    //Clean event dictionary
			    this._eventsDictionary.Clear();
		    }
	    }
	}
}
