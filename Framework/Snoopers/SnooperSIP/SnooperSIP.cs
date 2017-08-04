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
using Netfox.SnooperSIP.Enums;
using Netfox.SnooperSIP.Models;
using Netfox.SnooperSIP.Models.Message;

namespace Netfox.SnooperSIP
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
        public override SnooperExportBase PrototypExportObject { get; } = new SnooperExportSIP();

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
			    var reader = new PDUStreamReader(stream, Encoding.Default);
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

	    /*private RTPExportedPayload[] ProcessRTP(IPEndPoint[] RTPAddresses, string callID)
		{
			//stays constant 
			var AppTAG = "RTP";
			//var rtp = CaptureProcessor.GetConversations(AppTAG);
			var result = new List<RTPExportedPayload>();
			var path = Path.Combine(this.ExportBaseDirectory, "SIP_Calls");

			//init from parsed info in SIP
			for (var index1 = 0; index1 < RTPAddresses.Length; ++index1)
			{
				for (var index2 = index1 + 1; index2 < RTPAddresses.Length; ++index2)
				{
					//Console.WriteLine("searching conversations: " + RTPAddresses[index1] + " -> " + RTPAddresses[index2]);
					//gets all related conversations
					var convs = this.CaptureProcessor.GetConversations(RTPAddresses[index1], RTPAddresses[index2], AppTAG);

					//Console.WriteLine("found " + convs.Length + " conversations");

					//do your magic ... 
					foreach (var conv in convs)
					{
						// Get stream
						var stream = conv.GetPDUStreamBasedProvider(EFcPDUProviderType.SingleMessage); // as Stream;

						//var fileUp = "call_" + callID + "-" + conv.SourceEndPoint + "-" + conv.TargetEndPoint; // + "-up";
						//var fileDown = "call_" + callID + "-" + conv.TargetEndPoint + "-" + conv.SourceEndPoint; // + "-down";
						//fileUp = fileUp.Replace(".", "_").Replace(":", "_");
						//fileDown = fileDown.Replace(".", "_").Replace(":", "_");
						//var fullPathUp = Path.Combine(path, fileUp);
						//var fullPathDown = Path.Combine(path, fileDown);

						//var _exportedPayloadUp = new RTPExportedPayload(conv.SourceEndPoint, conv.TargetEndPoint, fullPathUp);
						//var _exportedPayloadDown = new RTPExportedPayload(conv.TargetEndPoint, conv.SourceEndPoint, fullPathDown);

						var _exportedPayloadsUp = new Dictionary<int, RTPExportedPayload>();
						var _exportedPayloadsDown = new Dictionary<int, RTPExportedPayload>();

						if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

						//TODO FIX if file exists
						//var fileStreamUp = File.Create(fullPathUp);
						//var fileStreamDown = File.Create(fullPathDown);

						var _fileStreamsUp = new Dictionary<int, FileStream>();
						var _fileStreamsDown = new Dictionary<int, FileStream>();

						do
						{
							var _rtpHeaderSize = 12;
							var _payloadType = -1;
							// Get real size of RTP header
							{
								var _byte = stream.ReadByte();
								if (_byte == -1)
								{
									// End of stream reached
									continue;
								}

								// Get Contributing source identifiers count
								var _csrcCount = _byte & 0x0f;

								// Get payload type
								_byte = stream.ReadByte();
								_payloadType = _byte & 0x7f;

								// Skip them  (each Contributing source identifiers is 4B)
								_rtpHeaderSize += _csrcCount * 4;
							}
							// Skip RTP header (_rtpHeaderSize now includes length of Contributing sources' identifiers
							stream.Seek(_rtpHeaderSize, SeekOrigin.Begin);

							// Write payload itself (excluding headers)
							switch (stream.GetCurrentPDU().FlowDirection)
							{
								case DaRFlowDirection.up:
									// Find appropriate structure
									if (_exportedPayloadsUp.ContainsKey(_payloadType))
									{
										// there's already output file there
										_exportedPayloadsUp[_payloadType].ProcessTimeStamp(stream.GetCurrentPDU().FirstSeen);
										stream.CopyTo(_fileStreamsUp[_payloadType]);
									}
									else
									{
										// record and output file of stream need to be created

										var _fileUp = "call_" + callID + "-" + conv.SourceEndPoint + "-" + conv.TargetEndPoint + "-" + _payloadType + "-up";
										_fileUp = _fileUp.Replace(".", "_").Replace(":", "_");
										var _fullPathUp = Path.Combine(path, _fileUp);

										_exportedPayloadsUp.Add(_payloadType, new RTPExportedPayload(conv.SourceEndPoint, conv.TargetEndPoint, _fullPathUp, _payloadType));
										_exportedPayloadsUp[_payloadType].ProcessTimeStamp(stream.GetCurrentPDU().FirstSeen);
										_fileStreamsUp.Add(_payloadType, File.Create(_fullPathUp));
										stream.CopyTo(_fileStreamsUp[_payloadType]);
									}
									break;
								case DaRFlowDirection.down:
									// Find appropriate structure
									if (_exportedPayloadsDown.ContainsKey(_payloadType))
									{
										// there's already output file there
										_exportedPayloadsDown[_payloadType].ProcessTimeStamp(stream.GetCurrentPDU().FirstSeen);
										stream.CopyTo(_fileStreamsDown[_payloadType]);
									}
									else
									{
										// record and output file of stream need to be created

										var _fileDown = "call_" + callID + "-" + conv.TargetEndPoint + "-" + conv.SourceEndPoint + "-" + _payloadType + "-down";
										_fileDown = _fileDown.Replace(".", "_").Replace(":", "_");
										var _fullPathDown = Path.Combine(path, _fileDown);

										_exportedPayloadsDown.Add(_payloadType, new RTPExportedPayload(conv.TargetEndPoint, conv.SourceEndPoint, _fullPathDown, _payloadType));
										_exportedPayloadsDown[_payloadType].ProcessTimeStamp(stream.GetCurrentPDU().FirstSeen);
										_fileStreamsDown.Add(_payloadType, File.Create(_fullPathDown));
										stream.CopyTo(_fileStreamsDown[_payloadType]);
									}
									break;
								default:
									//TODO throw exception, this should never happen
									break;
							}
						} while (stream.NewMessage());

						// close all FileStreams
						foreach (var file in _fileStreamsUp) { file.Value.Close(); }
						foreach (var file in _fileStreamsDown) { file.Value.Close(); }

						// try converting payloads
						foreach (var payload in _exportedPayloadsUp)
						{
							var _errorDesc = string.Empty;

							if (!payload.Value.TryConverting(out _errorDesc))
							{
								this.SnooperExport.AddExportReport(FcLogLevel.Warn, "RTP2WAV", "converting RTP payload to WAV failed",
									//ex.Message);
									_errorDesc);
								//Console.WriteLine("converting RTP payload to WAV failed :" + _errorDesc);
							}
						}
						foreach (var payload in _exportedPayloadsDown)
						{
							var _errorDesc = string.Empty;

							if (!payload.Value.TryConverting(out _errorDesc))
							{
								this.SnooperExport.AddExportReport(FcLogLevel.Warn, "RTP2WAV", "converting RTP payload to WAV failed",
									//ex.Message);
									_errorDesc);
								//Console.WriteLine("converting RTP payload to WAV failed :" + _errorDesc);
							}
						}

						// add gathered info structures to result
						result.AddRange(_exportedPayloadsUp.Values);
						result.AddRange(_exportedPayloadsDown.Values);
					}
				}
			}
			// Return processed RTP streams
			return result.ToArray();
		}*/
	}
}
