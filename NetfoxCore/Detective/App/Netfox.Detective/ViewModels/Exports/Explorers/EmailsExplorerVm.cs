using Castle.Windsor;
using Netfox.Detective.ViewModelsDataEntity.Exports;

namespace Netfox.Detective.ViewModels.Exports.Explorers
{
    public class EmailsExplorerVm : DetectiveViewModelBase
    {
        public EmailsExplorerVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
        }
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