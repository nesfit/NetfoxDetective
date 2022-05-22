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
using System.Security.Principal;

namespace Netfox.AnalyzerAppIdent.Services
{
    class Capturer
    {
        static ulong Count;
        IntPtr capEngine;
        IntPtr capFile;
        uint size;
        string filename;

        private CaptureCallbackDelegate pCaptureCallBack = new CaptureCallbackDelegate(CaptureCallback);

        public Capturer(string filename) { this.filename = filename; }

        public void Start()
        {
            uint size;
            uint errno;

            // Creates a capture file which will store the last 10MB of traffic captured
            errno = NetmonAPI.NmCreateCaptureFile(this.filename, 10000000, NmCaptureFileFlag.WrapAround, out this.capFile, out size);

            if(errno != 0) { Console.Write("Error Creating File"); }


            errno = NetmonAPI.NmOpenCaptureEngine(out this.capEngine);
            if(errno != 0) { Console.Write("Error Creating Engine"); }

            // The "1" below represents the index of the adapter to capture on, this is just a simple example which captures on one fixed adapter.
            errno = NetmonAPI.NmConfigAdapter(this.capEngine, 1, this.pCaptureCallBack, this.capFile, NmCaptureCallbackExitMode.DiscardRemainFrames);
            if(errno != 0) { Console.Write("Error Configuring Capture"); }
       }

        public void Stop()
        {
            uint errno;

            errno = NetmonAPI.NmStopCapture(this.capEngine, 1);
            if(errno != 0) { Console.Write("Error Stopping Capture"); }


            NetmonAPI.NmCloseHandle(this.capEngine);
            this.capEngine = IntPtr.Zero;

            NetmonAPI.NmCloseHandle(this.capFile);
            this.capFile = IntPtr.Zero;
        }

        private static void CaptureCallback(IntPtr hCaptureEngine, UInt32 ulAdapterIndex, IntPtr pCallerContext, IntPtr hFrame)
        {
            if(pCallerContext != IntPtr.Zero)
            {
                Count++;
                NetmonAPI.NmAddFrame(pCallerContext, hFrame);
                Console.WriteLine("Saved " + Count + " Frames");
            }
        }
        public static CaptureCallbackDelegate capHandler;
        static void CapHandlerCallback(IntPtr hCaptureEngine, UInt32 uladapterIndex, IntPtr pCallerContext, IntPtr hFrame)
        {
        }
        public void Test()
        {
            bool isElevated;
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

            Console.WriteLine($"Elevated permissions: {isElevated}");


            capHandler = new CaptureCallbackDelegate(CapHandlerCallback);
            uint ret;

            IntPtr myCapEng;
            ret = NetmonAPI.NmOpenCaptureEngine(out myCapEng);
            if (ret != 0)
            {
                Console.WriteLine("Error {0}\n", ret);
            }
            else
            {
                uint AdptCount;
                ret = NetmonAPI.NmGetAdapterCount(myCapEng, out AdptCount);
                if (ret != 0)
                {
                    NetmonAPI.NmCloseHandle(myCapEng);
                    Console.WriteLine("Error {0}\n", ret);
                }
                else
                {
                    // Creates a capture file which will store the last 10MB of traffic captured
                    ret = NetmonAPI.NmCreateCaptureFile(this.filename, 10000000, NmCaptureFileFlag.WrapAround, out this.capFile, out this.size);

                    if (ret != 0) { Console.Write("Error Creating File"); }

                    Console.WriteLine($"Adapters avalable: {AdptCount}");
                    for (uint i = 0; i < AdptCount; i++)
                    {
                        ret = NetmonAPI.NmConfigAdapter(myCapEng, i, capHandler, IntPtr.Zero, NmCaptureCallbackExitMode.ReturnRemainFrames);
                        if (ret != 0)
                        {
                            Console.WriteLine("Could not config {0}, error {1}", i, ret);
                        }
                        else
                        {
                            Console.WriteLine("Configured Adpt {0}", i);
                        }

                        ret = NetmonAPI.NmStartCapture(myCapEng, i, NmCaptureMode.Promiscuous);
                        if (ret != 0)
                        {
                            Console.WriteLine("Could not Start Capture on {0}, error {1}", i, ret);
                        }
                        else
                        {
                            Console.WriteLine("Started Adpt {0}", i);
                        }

                    }

                    System.Threading.Thread.Sleep(5000);

                    for (uint i = 0; i < AdptCount; i++)
                    {
                        ret = NetmonAPI.NmConfigAdapter(myCapEng, i, capHandler, IntPtr.Zero, NmCaptureCallbackExitMode.ReturnRemainFrames);
                        if (ret != 0)
                        {
                            Console.WriteLine("Could not config {0}, error {1}", i, ret);
                        }
                        else
                        {
                            Console.WriteLine("Configured Adpt {0}", i);
                        }
                        Console.WriteLine("Starting Adpt {0} again", i);

                        ret = NetmonAPI.NmStartCapture(myCapEng, i, NmCaptureMode.Promiscuous);
                        if (ret != 0)
                        {
                            Console.WriteLine("Could not Start Capture again on {0}, error {1}", i, ret);
                        }
                        else
                        {
                            Console.WriteLine("Started Adpt {0} again", i);
                        }

                    }

                    for (uint i = 0; i < AdptCount; i++)
                    {
                        ret = NetmonAPI.NmStopCapture(myCapEng, i);
                        if (ret != 0)
                        {
                            Console.WriteLine("Could not Stop Capture on {0}, error {1}", i, ret);
                        }
                        else
                        {
                            Console.WriteLine("Stopped Adpt {0}", i);
                        }
                    }

                }

                NetmonAPI.NmCloseHandle(myCapEng);
            }
        }
    }
}