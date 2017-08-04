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

namespace Netfox.Core.Messages.Base
{
    public class FrameMessage : DetectiveMessage
    {
        public enum MessageType
        {
            CurrentFrameChanged,
            //CurrentFrameByCaptureIdAndConvIndex,
            //CurrentFrameByExportedDataResult
        };

        private FrameMessage() { this.Sender = "FrameMessage"; }
        public object Frame { get; set; }
        public uint FrameId { get; set; }
        public string ExportResultId { get; set; }
        public string CaptureId { get; set; }
        public uint ConversationIndex { get; set; }
        public bool BringToFront { get; set; }
        public MessageType Type { get; set; }

        public static void SendFrameMessage(object frame, MessageType type, bool bringToFront)
        {
            var newFrameMessage = new FrameMessage
            {
                Frame = frame,
                Type = type,
                BringToFront = bringToFront
            };

            AsyncSendMessage(newFrameMessage);
        }

        public static void SendFrameMessage(string captureId, uint conversationIndex, uint frameId, MessageType type, bool bringToFront)
        {
            var newFrameMessage = new FrameMessage
            {
                CaptureId = captureId,
                ConversationIndex = conversationIndex,
                FrameId = frameId,
                Type = type,
                BringToFront = bringToFront
            };

            AsyncSendMessage(newFrameMessage);
        }

        public static void SendFrameMessage(string exportResultId, uint frameId, MessageType type, bool bringToFront)
        {
            var newFrameMessage = new FrameMessage
            {
                ExportResultId = exportResultId,
                FrameId = frameId,
                Type = type,
                BringToFront = bringToFront
            };

            AsyncSendMessage(newFrameMessage);
        }
    }
}