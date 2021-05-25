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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Netfox.Detective.Core.BaseTypes.Views;

namespace Netfox.Detective.Views.Windows
{
    /// <summary>
    ///     Interaction logic for RadMessageBox.xaml
    ///     This is the RadMessageBox dialog class itself
    /// </summary>
    /// <remarks>
    ///     This code was implemented by MasterSoft Software Solutions Ltd.
    ///     Visit the <see href="http://www.mastersoft.at">MasterSoft website</see> for further information.
    ///     <para>Dev: BGH -- Mag. (FH) Christian Kleinheinz</para>
    /// </remarks>
    public partial class RadMessageBox : DetectiveWindowBase, INotifyPropertyChanged
    {
        #region Constructors
        /// <summary>
        ///     Default constructor
        ///     The data context of the dialog is set to itself
        /// </summary>
        public RadMessageBox()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }
        #endregion

        #region Attributes
        /// <summary>
        ///     The result of this message box
        /// </summary>
        public MessageBoxResult Result { get; set; }
        #endregion

        #region DependencyProperties
        /// <summary>
        ///     This is the static buttons dependency property
        /// </summary>
        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.RegisterAttached("Buttons", typeof(MessageBoxButton), typeof(RadMessageBox),
            new PropertyMetadata(MessageBoxButton.OK, OnButtonsChanged));

        /// <summary>
        ///     Set or get the button mode of the message box to show.
        ///     The MessageBoxButton enumartion of microsoft is used.
        /// </summary>
        public MessageBoxButton Buttons
        {
            get { return ((MessageBoxButton) this.GetValue(ButtonsProperty)); }
            set
            {
                this.SetValue(ButtonsProperty, value);
                this.OnPropertyChanged("Buttons");
            }
        }

        /// <summary>
        ///     This is the static buttons dependency property
        /// </summary>
        public static readonly DependencyProperty DialogImageProperty = DependencyProperty.RegisterAttached("DialogImage", typeof(MessageBoxImage), typeof(RadMessageBox),
            new PropertyMetadata(MessageBoxImage.Warning, OnDialogImageChanged));

        /// <summary>
        ///     Set or get the button mode of the message box to show.
        ///     The MessageBoxButton enumartion of microsoft is used.
        /// </summary>
        public MessageBoxImage DialogImage
        {
            get { return ((MessageBoxImage) this.GetValue(DialogImageProperty)); }
            set
            {
                this.SetValue(DialogImageProperty, value);
                this.OnPropertyChanged("DialogImage");
            }
        }
        #endregion

        #region StaticDependencyPropChangeHandler
        /// <summary>
        ///     Event handler on changing the button mode of the message box
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private static void OnButtonsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {}

        /// <summary>
        ///     Event handler on changing the image dependency property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private static void OnDialogImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {}
        #endregion

        #region EventHandler
        /// <summary>
        ///     Button ok click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.OK;
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        ///     Button yes click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.Yes;
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        ///     Button no click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.No;
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        ///     Button cancel click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.Cancel;
            this.DialogResult = false;
            this.Close();
        }
        #endregion

        #region PropertyChanges
        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Property changed event handler
        /// </summary>
        /// <param name="propertyName">The name of the changed property</param>
        private void OnPropertyChanged(string propertyName)
        {
            if(this.PropertyChanged != null) { this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        }
        #endregion

        #region StaticMethods
        /// <summary>
        ///     Show a modal rad alert box
        ///     This static method was implemented following the Microsoft pattern for the standard WPF MessageBox
        ///     but is using the telerik RadWindow internally
        /// </summary>
        /// <param name="ctrlOwner">The owner window if needed otherwise null</param>
        /// <param name="strMessage">The message to display</param>
        /// <param name="strCaption">The title of the message box window. (Parameter is optional)</param>
        /// <param name="button">The buttons the dialog should have - Default is ok</param>
        /// <param name="image">The image the dialog should hold - Default is Warning</param>
        /// <returns>A message box result enum with the ResultState of the dialog</returns>
        public static MessageBoxResult Show(
            ContentControl ctrlOwner,
            string strMessage,
            string strCaption = null,
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.Warning)
        {
            try
            {
                var dlg = new RadMessageBox();
                dlg.DialogImage = image;
                dlg.Buttons = button;

                if(strCaption != null) { dlg.Header = strCaption; }

                dlg.txtMessage.Text = strMessage;

                if(ctrlOwner != null) { dlg.Owner = ctrlOwner; }

                dlg.ShowDialog();

                var res = dlg.Result;
                return (res);
            }
            catch(Exception err)
            {
                Trace.TraceError(err.ToString());
                return (MessageBoxResult.None);
            }
        }

        /// <summary>
        ///     Show a modal RadMessageBox with a message and the window title as option
        /// </summary>
        /// <param name="strMessage">The message string to show</param>
        /// <param name="strCaption">The title of the message box window.(Parameter is optional)</param>
        public static void Show(string strMessage, string strCaption = null)
        {
            Show(null, strMessage, strCaption);
        }

        /// <summary>
        ///     Show a modal RadMessageBox with a message a window title and configure the buttons to display
        /// </summary>
        /// <param name="strMessage">The message string to show</param>
        /// <param name="strCaption">The title of the message box window.(Parameter is optional)</param>
        /// <param name="button">The buttons the dialog should have - Default is ok</param>
        public static void Show(string strMessage, string strCaption, MessageBoxButton button)
        {
            Show(null, strMessage, strCaption, button);
        }

        /// <summary>
        ///     Show a modal RadMessageBox with a message a window title, configure the buttons to display and set the dialogs
        ///     image
        /// </summary>
        /// <param name="strMessage">The message string to show</param>
        /// <param name="strCaption">The title of the message box window.(Parameter is optional)</param>
        /// <param name="button">The buttons the dialog should have - Default is ok</param>
        /// <param name="image">The image the dialog should hold - Default is Warning</param>
        public static MessageBoxResult Show(string strMessage, string strCaption, MessageBoxButton button, MessageBoxImage image)
        {
            return (Show(null, strMessage, strCaption, button, image));
        }

        public static bool IsFocusable
        {
            get { return true; }
        }
        #endregion
    }
}