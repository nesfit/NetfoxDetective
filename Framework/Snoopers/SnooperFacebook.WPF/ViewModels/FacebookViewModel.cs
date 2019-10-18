// Copyright (c) 2017 Jan Pluskal, Tomas Bruckner
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
using Netfox.SnooperFacebook.Models.Files.Group;
using Netfox.SnooperFacebook.Models.Files.Messenger;
using Netfox.SnooperFacebook.Models.Statuses;
using Netfox.SnooperFacebook.Models.Text;
using Netfox.SnooperFacebook.WPF.Views.Interface;

namespace Netfox.SnooperFacebook.WPF.ViewModels
{
    public class FacebookViewModel : DetectiveExportDetailPaneViewModelBase
    {
        public IEnumerable<FacebookComment> Comments { get; set; }
        public IEnumerable<FacebookMessage> Messages { get; set; }
        public IEnumerable<FacebookGroupMessage> GroupMessages { get; set; }
        public IEnumerable<FacebookStatus> Statuses { get; set; }
        public IEnumerable<FacebookMessengerPhoto> Photos { get; set; }
        public IEnumerable<FacebookMessengerFile> Files { get; set; }
        public IEnumerable<FacebookMessengerGroupPhoto> GroupPhotos { get; set; }
        public IEnumerable<FacebookMessengerGroupFile> GroupFiles { get; set; }

        public override string HeaderText => "Facebook detail";

        public FacebookViewModel(WindsorContainer applicationWindsorContainer, ExportVm model, IFacebookView view) : base(applicationWindsorContainer, model, view)
        {
            try
            {

                    this.Comments = model.SnooperExportedObjects.Where(i => i is FacebookComment).Cast<FacebookComment>().ToArray();
                    this.Messages = model.SnooperExportedObjects.Where(i => i is FacebookMessage).Cast<FacebookMessage>().ToArray();
                    this.GroupMessages = model.SnooperExportedObjects.Where(i => i is FacebookGroupMessage).Cast<FacebookGroupMessage>().ToArray();
                    this.Statuses = model.SnooperExportedObjects.Where(i => i is FacebookStatus).Cast<FacebookStatus>().ToArray();
                    this.Photos = model.SnooperExportedObjects.Where(i => i is FacebookMessengerPhoto).Cast<FacebookMessengerPhoto>().ToArray();
                    this.Files = model.SnooperExportedObjects.Where(i => i is FacebookMessengerFile).Cast<FacebookMessengerFile>().ToArray();
                    this.GroupPhotos = model.SnooperExportedObjects.Where(i => i is FacebookMessengerGroupPhoto).Cast<FacebookMessengerGroupPhoto>().ToArray();
                    this.GroupFiles = model.SnooperExportedObjects.Where(i => i is FacebookMessengerGroupFile).Cast<FacebookMessengerGroupFile>().ToArray();

                    if(this.AnyObjects())
                    {
                        this.IsActive = true;
                        this.IsHidden = false;
                    }
                    else
                    {
                        this.IsHidden = true;
                        this.IsActive = false;
                    }

                    //this.ExportVmObserver.RegisterHandler(p => p.SelectedSnooperExportObject, p => this.OnPropertyChanged(nameof(this.SelectedComment)));

            }
            catch (Exception ex)
            {
                this.Logger?.Error($"{this.GetType().Name} instantiation failed", ex);
            }
        }

        private bool AnyObjects()
        {
            return this.Comments.Any() || this.Messages.Any() || this.Statuses.Any() || this.GroupMessages.Any() || this.Files.Any() || this.GroupFiles.Any() || this.Photos.Any()
                   || this.GroupPhotos.Any();
        }
    }
}