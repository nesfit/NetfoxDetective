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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Core.Interfaces.Views.Exports;
using Netfox.Detective.Core;
using Netfox.Detective.ViewModelsDataEntity.Exports.ModelWrappers;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.Detail
{
    [NotifyPropertyChanged]
    public class EmailDetailVm : DetectiveExportDetailPaneViewModelBase
    {
        public EmailDetailVm(WindsorContainer applicationWindsorContainer, ExportVm model, IEmailDetailView view) : base(applicationWindsorContainer, model, view)
        {
            try
            {
                this.IsHidden = !this.ExportVm.Emails?.Any() ?? true;
                this.IsActive = this.ExportVm.Emails?.Any() ?? false;
                this.DockPositionPosition = DetectiveDockPosition.DockedDocument;
                this.ExportVmObserver.RegisterHandler(p => p.SelectedSnooperExportObject, p =>
                {
                    if(p.SelectedSnooperExportObject is IEMail)
                    {
                        this.SelectedEmail = this.ApplicationOrInvestigationWindsorContainer.Resolve<EmailVm>(new
                        {
                            model = p.SelectedSnooperExportObject as IEMail,
                            investigationOrAppWindsorContainer = this.ApplicationOrInvestigationWindsorContainer
                        });
                        this.IsHidden = false;
                        this.IsActive = true;
                        this.SelectedBodieIndex = 0;
                        ///Studiod hack to bypas Telerik malfunctioning tab selection
                        Task.Run(() =>
                        {
                            Thread.Sleep(500);
                            this.SelectedBodieIndex = 0;
                        });
                    }
                    else
                    {
                        this.SelectedEmail = null;
                        this.IsHidden = true;
                        this.IsActive = false;
                    }
                });
            }
            catch(Exception ex) {
                this.Logger?.Error($"{this.GetType().Name} instantiation failed", ex);
            }
        }

        #region Overrides of DetectivePaneViewModelBase
        public override string HeaderText => "Email detail";
        #endregion

        public int SelectedBodieIndex { get; set; }

        public EmailVm SelectedEmail { get; private set; }
    }
}