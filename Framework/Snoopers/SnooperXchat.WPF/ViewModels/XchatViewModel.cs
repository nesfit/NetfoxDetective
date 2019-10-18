// Copyright (c) 2017 Jan Pluskal, Dudek Jindrich
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
using System.Linq;
using Castle.Windsor;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.ViewModelsDataEntity.Exports.Detail;
using Netfox.SnooperXchat.Models.Text;
using Netfox.SnooperXchat.WPF.Views.Interface;

namespace Netfox.SnooperXchat.WPF.ViewModels
{
    /// <summary>
    /// Class for XchatViewModel
    /// </summary>
    public class XchatViewModel : DetectiveExportDetailPaneViewModelBase
    {
        public IEnumerable<XChatPrivateMessage> PrivateMessages { get; set; }
        public IEnumerable<XChatRoomMessage> RoomMessages { get; set; }

        public override string HeaderText => "Xchat detail";

        public XchatViewModel(WindsorContainer applicationOrAppWindsorContainer, ExportVm model, IXChatView view) : base(applicationOrAppWindsorContainer, model, view)
        {
            try
            {
                this.PrivateMessages = model.SnooperExportedObjects.Where(i => i is XChatPrivateMessage).Cast<XChatPrivateMessage>().OrderBy(it => it.Time).ToArray();
                this.RoomMessages = model.SnooperExportedObjects.Where(i => i is XChatRoomMessage).Cast<XChatRoomMessage>().OrderBy(it => it.Time).ToArray();

                if (this.AnyObjects())
                {
                    this.IsActive = true;
                    this.IsHidden = false;
                }
                else
                {
                    this.IsHidden = true;
                    this.IsActive = false;
                }
            }
            catch (Exception ex)
            {
                this.Logger?.Error($"{this.GetType().Name} instantiation failed", ex);
            }
        }

        private bool AnyObjects()
        {
            return this.PrivateMessages.Any() || this.RoomMessages.Any();
        }
    }
}