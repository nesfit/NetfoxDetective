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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Netfox.AnalyzerSIPFraud.Models;
using Newtonsoft.Json;

namespace Netfox.AnalyzerSIPFraud.Services
{
    public class NemeaProxy : IDisposable
    {
        public NetworkStream NetworkStream { get; private set; }

        

        #region Implementation of IDisposable
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ((IDisposable) this.TcpClient)?.Dispose();
            this.NetworkStream?.Dispose();
        }
        #endregion

        public async Task Connect(Uri url)
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // This example uses port 11000 on the local computer.

                //url = new Uri("http://sauvignon.liberouter.org:9999/");
                //url = new Uri("telnet://sauvignon.liberouter.org:9999/");

                var ipAddress = url.HostNameType == UriHostNameType.Dns? Dns.GetHostEntry(url.Host).AddressList[0] : IPAddress.Parse(url.Host);
                var remoteEp = new IPEndPoint(ipAddress, url.Port);
                
                this.TcpClient = new TcpClient();

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    await this.TcpClient.ConnectAsync(remoteEp.Address,remoteEp.Port);
                    //this.Socket.ConnectAsync(remoteEp);
                    this.NetworkStream = this.TcpClient.GetStream();

                    //Console.WriteLine("Socket connected to {0}",
                    //    sender.RemoteEndPoint.ToString());

                    //// Encode the data string into a byte array.
                    //var msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                    //// Send the data through the socket.
                    //var bytesSent = sender.Send(msg);

                    //// Receive the response from the remote device.
                    //var bytesRec = sender.Receive(bytes);
                    //Console.WriteLine("Echoed test = {0}",
                    //    Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    //// Release the socket.
                    //sender.Shutdown(SocketShutdown.Both);
                    //sender.Close();
                }
                catch(ArgumentNullException ane) {
                    Console.WriteLine("ArgumentNullException : {0}", ane);
                }
                catch(SocketException se) {
                    Console.WriteLine("SocketException : {0}", se);
                }
                catch(Exception e) {
                    Console.WriteLine("Unexpected exception : {0}", e);
                }
            }
            catch(Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public TcpClient TcpClient { get; set; }

        public void Disconnect()
        {
            try
            {
                //// Release the socket.
                this.TcpClient.Close();
                this.TcpClient.Dispose();
                this.TcpClient = null;
                this.NetworkStream.Close();
                this.NetworkStream.Dispose();
                this.NetworkStream = null;
            }
            catch(ObjectDisposedException ane) {
                Console.WriteLine("ObjectDisposedException : {0}", ane);
            }
            catch(SocketException se) {
                Console.WriteLine("SocketException : {0}", se);
            }
            catch(Exception e) {
                Console.WriteLine("Unexpected exception : {0}", e);
            }
        }

        //public string GetJsonMessage()
        //{
        //    var buffer = new char[8192];
        //    int readBytes;
        //    using (var textReader = new StreamReader(this.NetworkStream))
        //    {

        //         readBytes = textReader.Read(buffer, 0, 8192);
        //    }
        //    return new string(buffer);
        //}

        public string GetJsonMessage()
        {
            //var message = new byte[4096];
            //var bytesRead = 0;
            //// Blocks until a client sends a message                    
            //bytesRead = this.NetworkStream.Read(message, 0, 4096);
            //return Encoding.Default.GetString(message);
            using (var textReader = new StreamReader(this.NetworkStream,Encoding.UTF8,true,4096,true)) { return textReader.ReadLine(); }

            
            
        }

        public JsonModels.Message GetMessage() { return this.Parse(this.GetJsonMessage()); }
        public async Task<JsonModels.Message> GetMessageAsync() { return await Task.Run(() =>
        {
            try { return this.Parse(this.GetJsonMessage()); }
            catch(Exception ex)
            {
                Task.Delay(1000);
                throw new SystemException("Backend service is not available! Check the URL.", ex);
            }
        }); }

        public JsonModels.Message Parse(string json) { return JsonConvert.DeserializeObject<JsonModels.Message>(json); }
    }
}