﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Netfox.Core.Database;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.SupportedTypes;

namespace Netfox.Framework.Models
{
    public class CaptureL4 : IEntity
    {
        private DateTime? _firstSeen;

        [Key, ForeignKey(nameof(L4Conversation))]
        public Guid Id { get; set; } = Guid.NewGuid();

        private CaptureL4()
        {
        }

        public CaptureL4(PmCaptureBase capture)
        {
            this.OriginalCapture = capture;
        }

        public String Hash { get; set; }
        public String FilePath { get; set; }
        public PmCaptureFileType CaptureFileType { get; set; }

        public Guid? OriginalCaptureRefId { get; set; } = Guid.Empty;

        [ForeignKey(nameof(OriginalCaptureRefId))]
        public virtual PmCaptureBase OriginalCapture { get; set; }

        public Guid? L4ConversationRefId { get; set; }

        [ForeignKey(nameof(L4ConversationRefId))]
        public virtual L4Conversation L4Conversation { get; set; }

        #region Implementation of IEntity

        public DateTime FirstSeen
        {
            get { return (DateTime) (this._firstSeen ?? (this._firstSeen = this.L4Conversation.FirstSeen)); }
            set { this._firstSeen = value; }
        }

        #endregion
    }
}