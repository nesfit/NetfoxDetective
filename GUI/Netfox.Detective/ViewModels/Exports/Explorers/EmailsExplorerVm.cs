// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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

using Castle.Windsor;
using Netfox.Detective.ViewModelsDataEntity.Exports;

namespace Netfox.Detective.ViewModels.Exports.Explorers
{
    public class EmailsExplorerVm : DetectiveViewModelBase
    {
        public EmailsExplorerVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer) { }
        //public EmailExportResultVm EmailExportResultContent { get; private set; }
        //public ExportDataVm ExportDataContext { get; set; }

        public ExportVm ExportResultContext
        {
            set
            {
                //todo
                //var emailExportResult = value.ModelSpecificCollections[typeof(EmailExportVm)] as EmailExportResultVm;

                //if(emailExportResult != null)
                //{
                //    this.EmailExportResultContent = emailExportResult;

                //    if(this.EmailExportResultContent != null && emailExportResult.Emails.Any())
                //    {
                //        this.IsHidden = false;

                //    }

                //}
            }
        }
    }
}