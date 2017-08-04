using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Netfox.Detective.Core.BaseTypes.Views;
using Netfox.Detective.Core.Messages;
using Netfox.Detective.Models;
using Netfox.Detective.ViewModelsDataEntity.BkTasks;
using Netfox.Framework.Interfaces;
using Telerik.Windows.Controls.Navigation;

namespace Netfox.Detective.Views.Windows
{
    /// <summary>
    ///     Interaction logic for ProtocolsRecognitionView.xaml
    /// </summary>
    public partial class ProtocolsRecognitionView : DetectiveWindowBase
    {
        private ProtocolsRecognitionContext _context;

        public ProtocolsRecognitionView(ProtocolsRecognitionContext context)
        {
            this.InitializeComponent();

            this._context = context;

            this.DataContext = this;

            RadWindowInteropHelper.SetShowInTaskbar(this, true);
            RadWindowInteropHelper.SetTitle(this, this.Header.ToString());
        }

        public static bool IsFocusable
        {
            get { return true; }
        }

        public string Scope
        {
            get
            {
                var scope = string.Empty;
                switch(this._context.RecognitionScope)
                {
                    case ProtocolsRecognitionContext.RecognitionScopeType.Investigation:
                        scope = "Investigation: " + this._context.InvestigationScope.Name;
                        break;
                    case ProtocolsRecognitionContext.RecognitionScopeType.Capture:
                        scope = "Capture: " + this._context.CaptureScope.FileName;
                        break;
                    case ProtocolsRecognitionContext.RecognitionScopeType.Conversation:
                        scope = "Conversation: " + this._context.Capture.Capture.FileInfo.Name + " ID:" + this._context.ConversationScope.Index;
                        break;
                }

                return scope;
            }
        }

        public IEnumerable<FwControllerContext.Recognizer> Recognizers
        {
            get { return this._context.Controller.AvialableRecognizers; }
        }

        public FwControllerContext.Recognizer SelectedRecognizer { get; set; }

        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            //todo to command
            Debugger.Break();
            //if (this.SelectedRecognizer != null)
            //{
            //    var bgTask = this.BgTasksManagerService.CreateTask("Protocols recognition", this.Scope);
            //    bgTask.Start(this.Start);

            //}
        }

        private void RecognizeConversation(IApplicationRecognizer recognizer, FwCaptureContext capture, Conversation conversation)
        {
            if(recognizer != null && conversation != null)
            {
                var fwConv = capture.Capture.GetConversation(conversation.Index);
                if(fwConv != null)
                {
                    var tags = fwConv.RecognizeApplicationProtocolsTags(recognizer);

                    if(tags != null && tags.Any())
                    {
                        conversation.AutoSave = false;
                        conversation.ApplicationTags = new string[tags.Count(s => !string.IsNullOrEmpty(s))];
                        var i = 0;
                        foreach(var tag in tags) { if(!string.IsNullOrEmpty(tag)) { conversation.ApplicationTags[i++] = tag.ToUpper(); } }

                        fwConv.ReTagApplicationProtocols(tags);
                        conversation.PersistenceProvider.Update(conversation);
                        conversation.AutoSave = true;
                    }
                }
            }
        }

        private void Start(object task)
        {
            var bgTask = task as BgTaskVm;
            if(bgTask == null) { return; }
            try
            {
                var recognizer = (IApplicationRecognizer) Activator.CreateInstance(this.SelectedRecognizer.ProtoType, new object[]
                {
                    false
                });


                switch(this._context.RecognitionScope)
                {
                    case ProtocolsRecognitionContext.RecognitionScopeType.Investigation:

                        bgTask.Target = 0;
                        foreach(var capture in this._context.InvestigationScope.Captures) { bgTask.Target += capture.Conversations.Count; }
                        bgTask.IsIndeterminate = false;

                        foreach(var capture in this._context.InvestigationScope.Captures)
                        {
                            foreach(var converastion in capture.Conversations)
                            {
                                this.RecognizeConversation(recognizer, capture.FwCaptureContext, converastion);
                                bgTask.IncrementProgress();
                            }

                            capture.TagsUpdated();
                        }

                        break;
                    case ProtocolsRecognitionContext.RecognitionScopeType.Capture:

                        bgTask.Target = this._context.CaptureScope.Conversations.Count;
                        bgTask.IsIndeterminate = false;

                        foreach(var conversation in this._context.CaptureScope.Conversations)
                        {
                            this.RecognizeConversation(recognizer, this._context.Capture, conversation);
                            bgTask.IncrementProgress();
                        }
                        ;

                        this._context.CaptureScope.TagsUpdated();

                        break;
                    case ProtocolsRecognitionContext.RecognitionScopeType.Conversation:

                        this.RecognizeConversation(recognizer, this._context.Capture, this._context.ConversationScope);

                        break;
                }
            }
            catch(Exception ex) {
                SystemMessage.SendSystemMessage(SystemMessage.Type.Error, "Advanced protocols recognition", ex.Message, "ProtocolsRecognition");
            }
        }
    }
}