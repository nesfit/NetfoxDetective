using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Netfox.Framework.Models.Snoopers.Email;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.ModelWrappers
{
    [NotifyPropertyChanged]
    public class MimePartVm : INotifyPropertyChanged
    {
        public MimePartVm(MIMEpart mimEpart)
        {
            this.MIMEpart = mimEpart;
        }

        public MIMEpart MIMEpart { get; private set; }

        public string Subject
        {
            get
            {
                if (this.MIMEpart == null)
                {
                    return String.Empty;
                }

                return this.MIMEpart.Subject;
            }
        }

        public string From
        {
            get
            {
                if (this.MIMEpart == null)
                {
                    return String.Empty;
                }

                return this.MIMEpart.From;
            }
        }

        public string To
        {
            get
            {
                if (this.MIMEpart == null)
                {
                    return String.Empty;
                }

                return this.MIMEpart.To;
            }
        }

        public string FileName => this.MIMEpart.SuggestedFilename;

        public string FilePath
        {
            get { return this.MIMEpart.StoredContent.FullName; }
        }

        public IEnumerable<MimePartVm> ChildrenParts
        {
            get
            {
                if (this.MIMEpart.ContainedParts == null)
                {
                    yield break;
                }

                foreach (var mimEpart in this.MIMEpart.ContainedParts)
                {
                    yield return new MimePartVm(mimEpart);
                }
            }
        }

        public string RawHeaderAndContent => this.RawHeader + Environment.NewLine + this.RawContent;

        public string RawHeader => this.RawHeader;

        public string RawContent => this.MIMEpart.RawContent;

        public void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}