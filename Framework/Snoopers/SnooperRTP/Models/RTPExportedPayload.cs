// Copyright (c) 2017 Jan Pluskal, Martin Kmet
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
using System.Net;
using System.Web.Hosting;

namespace Netfox.SnooperRTP.Models
{
    public class RTPExportedPayload
    {
        private const string SoxPath = "tools\\SoX.14.3.2\\sox.exe";
        private const string DecodersPath = "tools\\RTPDecoders\\";
        private const string DecoderG723_1Path = DecodersPath + "G723.1_decode.exe";
        private const string DecoderG729APath = DecodersPath + "G729A_decode.exe";
        private const string DecoderG729Path = DecodersPath + "G729_decode.exe";
        private const string DecoderG728Path = DecodersPath + "G728_decode.exe";
        private const string DecoderG726Path = DecodersPath + "G726_decode.exe";
        private const string DecoderG722Path = DecodersPath + "G722_decode.exe";
        public string ConvertedFilename;
        public IPEndPoint Destination;
        public DateTime? End;
        public string Filename;
        public int PayloadType;
        public IPEndPoint Source;
        public DateTime? TimeStamp;

        private RTPExportedPayload() { } //EF
        public RTPExportedPayload(IPEndPoint source, IPEndPoint destination, string filename, int payloadType)
        {
            this.TimeStamp = null;
            this.End = null;
            this.Source = source;
            this.Destination = destination;
            this.Filename = filename;
            this.ConvertedFilename = string.Empty;
            this.PayloadType = payloadType;
        }

        public void ProcessTimeStamp(DateTime timeStamp)
        {
            if(!this.TimeStamp.HasValue) { this.TimeStamp = timeStamp; }
            this.End = timeStamp;
        }

        public override string ToString()
        {
            var converted = string.Empty;
            var _begin = (DateTime) this.TimeStamp;
            var _end = (DateTime) this.End;
            if(this.TimeStamp.HasValue)
            {
                converted += "  stream " + this.Source + " -> " + this.Destination;
                converted += "\n   Begin:    " + _begin.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                converted += "\n   End:      " + _end.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                converted += "\n   Duration: " + (_end - _begin).ToString(@"d' days, 'hh\:mm\:ss\.ffffff");
                converted += "\n   Payload:  " + this.PayloadType;
                converted += "\n   Filename: " + this.Filename;
                if(this.ConvertedFilename != string.Empty) { converted += "\n   Wav file: " + this.ConvertedFilename; }
            }
            return converted;
        }

        public static string TryConverting(int payloadType, string rawFilePath, out string whatWentWrong)
        {
            whatWentWrong = string.Empty;
            // u-law, a-law or gsm
            if(payloadType == 0 || payloadType == 8 || payloadType == 3)
            {
                var _type = string.Empty;
                var _encoding = string.Empty;

                switch(payloadType)
                {
                    case 0: // u-law
                        _type = "raw";
                        _encoding = "mu-law";
                        break;
                    case 8: // a-law
                        _type = "raw";
                        _encoding = "a-law";
                        break;
                    case 3: // gsm
                        _type = "gsm";
                        _encoding = "gsm-full-rate";
                        break;
                }

                var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                p.StartInfo.FileName = HostingEnvironment.ApplicationPhysicalPath + "bin\\" + SoxPath;
                p.StartInfo.Arguments = "--type " + _type + " --rate 8000 --encoding " + _encoding + " \"" + rawFilePath + "\" --type wavpcm -s --channels 1 \"" + rawFilePath
                                        + ".wav\"";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                if(output == string.Empty)
                {
                    // ConvertedFilename = rawFilePath + ".wav";
                }
                else
                {
                    whatWentWrong = "converting of file \"" + rawFilePath + "\" to wav failed";
                    return null;
                }
            }

            // G.722, G.728 or G.729
            if(payloadType == 9 || payloadType == 15 || payloadType == 18)
            {
                var _decoderPath = string.Empty;

                switch(payloadType)
                {
                    case 9:
                        _decoderPath = DecoderG722Path;
                        break;
                    case 15:
                        _decoderPath = DecoderG728Path;
                        break;
                    case 18:
                        _decoderPath = DecoderG729Path;
                        break;
                }

                // Phase 1: convert payload to linear PCM
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                p.StartInfo.FileName = HostingEnvironment.ApplicationPhysicalPath + "bin\\" + _decoderPath;
                p.StartInfo.Arguments = "\"" + rawFilePath + "\" \"" + rawFilePath + ".s16\"";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                // Error occured
                if(output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    whatWentWrong = "converting of file " + rawFilePath + " to wav failed (phase 1)";
                    return null;
                }

                // Phase 2: convert linear PCM to WAV
                p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                p.StartInfo.FileName = HostingEnvironment.ApplicationPhysicalPath + "bin\\" + SoxPath;
                p.StartInfo.Arguments = "--rate 8000 --channels 1 \"" + rawFilePath + ".s16\" --type wavpcm -s \"" + rawFilePath + ".wav\"";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                if(output == string.Empty)
                {
                    //ConvertedFilename = rawFilePath + ".wav";
                }
                else
                {
                    whatWentWrong = "converting of file \"" + rawFilePath + "\" to wav failed (phase 2)";
                    return null;
                }
            }

            return rawFilePath + ".wav";
        }

        public bool TryConverting(out string whatWentWrong)
        {
            //Console.WriteLine(Directory.GetCurrentDirectory());
            whatWentWrong = string.Empty;
            // u-law, a-law or gsm
            if(this.PayloadType == 0 || this.PayloadType == 8 || this.PayloadType == 3)
            {
                var _type = string.Empty;
                var _encoding = string.Empty;

                switch(this.PayloadType)
                {
                    case 0: // u-law
                        _type = "raw";
                        _encoding = "mu-law";
                        break;
                    case 8: // a-law
                        _type = "raw";
                        _encoding = "a-law";
                        break;
                    case 3: // gsm
                        _type = "gsm";
                        _encoding = "gsm-full-rate";
                        break;
                }

                var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = SoxPath,
                        Arguments =
                            "--type " + _type + " --rate 8000 --encoding " + _encoding + " \"" + this.Filename + "\" --type wavpcm -s --channels 1 \"" + this.Filename + ".wav\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };

                p.Start();

                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                if(output == string.Empty) {
                    this.ConvertedFilename = this.Filename + ".wav";
                }
                else
                {
                    whatWentWrong = "converting of file \"" + this.Filename + "\" to wav failed";
                    return false;
                }
            }

            // G.722, G.728 or G.729
            if(this.PayloadType == 9 || this.PayloadType == 15 || this.PayloadType == 18)
            {
                var _decoderPath = string.Empty;

                switch(this.PayloadType)
                {
                    case 9:
                        _decoderPath = DecoderG722Path;
                        break;
                    case 15:
                        _decoderPath = DecoderG728Path;
                        break;
                    case 18:
                        _decoderPath = DecoderG729Path;
                        break;
                }

                // Phase 1: convert payload to linear PCM
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                p.StartInfo.FileName = HostingEnvironment.ApplicationPhysicalPath + "bin\\" + _decoderPath;
                p.StartInfo.Arguments = "\"" + this.Filename + "\" \"" + this.Filename + ".s16\"";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                // Error occured
                if(output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    whatWentWrong = "converting of file " + this.Filename + " to wav failed (phase 1)";
                    return false;
                }

                // Phase 2: convert linear PCM to WAV
                p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                p.StartInfo.FileName = HostingEnvironment.ApplicationPhysicalPath + "bin\\"  + SoxPath;
                p.StartInfo.Arguments = "--rate 8000 --channels 1 \"" + this.Filename + ".s16\" --type wavpcm -s \"" + this.Filename + ".wav\"";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                if(output == string.Empty) {
                    this.ConvertedFilename = this.Filename + ".wav";
                }
                else
                {
                    whatWentWrong = "converting of file \"" + this.Filename + "\" to wav failed (phase 2)";
                    return false;
                }
            }

            return true;
        }

        public bool Valid() { return this.TimeStamp.HasValue; }
    }
}