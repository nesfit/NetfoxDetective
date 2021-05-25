using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using AlphaChiTech.VirtualizingCollection.Interfaces;
using Netfox.Core.Collections;
using Netfox.Core.Database;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Detective.Models.Conversations
{
    /// <summary>
    ///     Class represents group of conversations
    /// </summary>
    [Persistent]
    public class ConversationsGroup : IConversationsModel, IEntity, INotifyPropertyChanged
    {
        public void AddConversation(ILxConversation conversation)
        {
            if (conversation == null)
            {
                return;
            }

            var conversationType = conversation.GetType();
            if (conversationType == typeof(L3Conversation))
            {
                this.L3Conversations.Add(conversation as L3Conversation);
            }
            else if (conversationType == typeof(L4Conversation))
            {
                this.L4Conversations.Add(conversation as L4Conversation);
            }
            else if (conversationType == typeof(L7Conversation))
            {
                this.L7Conversations.Add(conversation as L7Conversation);
            }
        }

        #region Constructors

        public ConversationsGroup()
        {
            this.Name = "Conversation group" + Guid.NewGuid();
            this.FirstSeen = DateTime.Now;
        }

        public ConversationsGroup(IEnumerable<ILxConversation> conversations) : this()
        {
            //todo fix this.Investigation = investigation;
            foreach (var conv in conversations)
            {
                this.AddConversation(conv);
            }
        }

        public ConversationsGroup(IConversationsModel conversationsModel) : this()
        {
            //todo fix this.Investigation = investigation;
            foreach (var conv in conversationsModel.L3Conversations)
            {
                this.L3Conversations.Add(conv);
            }

            foreach (var conv in conversationsModel.L4Conversations)
            {
                this.L4Conversations.Add(conv);
            }

            foreach (var conv in conversationsModel.L7Conversations)
            {
                this.L7Conversations.Add(conv);
            }
        }

        #endregion

        #region Properties

        private string _name;

        public string Name
        {
            get { return this._name ?? (this._name = Guid.NewGuid().ToString()); }
            set
            {
                this._name = value;
                this.OnPropertyChanged();
            }
        }

        private string _description;

        public string Description
        {
            get { return this._description; }
            set
            {
                this._description = value;
                this.OnPropertyChanged();
            }
        }

        [NotMapped]
        public IEnumerable<ILxConversation> Conversations =>
            this.L3Conversations.Concat(
                this.L4Conversations.Concat<ILxConversation>(this.L7Conversations)); //todo thread safe

        //todo implement to take from all frames
        [NotMapped]
        public IObservableCollection<PmFrameBase> Frames { get; } =
            new ConcurrentIObservableVirtualizingObservableCollection<PmFrameBase>();

        [NotMapped]
        public virtual IObservableCollection<L3Conversation> L3Conversations { get; private set; } =
            new ConcurrentIObservableVirtualizingObservableCollection<L3Conversation>();

        [NotMapped]
        public virtual IObservableCollection<L4Conversation> L4Conversations { get; private set; } =
            new ConcurrentIObservableVirtualizingObservableCollection<L4Conversation>();

        [NotMapped]
        public virtual IObservableCollection<L7Conversation> L7Conversations { get; private set; } =
            new ConcurrentIObservableVirtualizingObservableCollection<L7Conversation>();

        [NotMapped]
        public virtual IObservableCollection<SnooperExportBase> SnooperExports { get; private set; } =
            new ConcurrentIObservableVirtualizingObservableCollection<SnooperExportBase>();

        [NotMapped]
        public virtual IObservableCollection<ISnooper> UsedSnoopers { get; } =
            new ConcurrentIObservableVirtualizingObservableCollection<ISnooper>();

        #endregion

        #region Implementation of IEntity

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember]
        public Guid Id { get; set; }

        public DateTime FirstSeen { get; set; }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}