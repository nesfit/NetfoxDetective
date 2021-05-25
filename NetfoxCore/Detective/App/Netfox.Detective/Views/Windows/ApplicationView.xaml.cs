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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Netfox.Core.Interfaces;
using Netfox.Core.Interfaces.Views;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;
using Telerik.Windows.Controls.Navigation;
using StateChangeEventArgs = Telerik.Windows.Controls.Docking.StateChangeEventArgs;

namespace Netfox.Detective.Views.Windows
{
    /// <summary>
    ///     Interaction logic for ApplicationView.xaml
    /// </summary>
    public partial class ApplicationView: IApplicationView
    {
        #region Constructor
        public ApplicationView(IApplicationShell applicationShell)
        {
            this.InitializeComponent();
            this.DataContext = applicationShell;
            RadWindowInteropHelper.SetShowInTaskbar(this, true);
            RadWindowInteropHelper.SetTitle(this, this.Header.ToString());
        }

        private void OnPreviewShowCompass(object sender, PreviewShowCompassEventArgs e)
        {
            var isRootCompass = e.Compass is RootCompass;
            var splitContainer = e.DraggedElement as RadSplitContainer;
            if(splitContainer != null)
            {
                var isDraggingDocument = splitContainer.EnumeratePanes().Any(p => p is RadDocumentPane);
                var isTargetDocument = e.TargetGroup == null || e.TargetGroup.EnumeratePanes().Any(p => p is RadDocumentPane);
                if(isDraggingDocument) {
                    e.Canceled = isRootCompass || !isTargetDocument;
                }
                else
                {
                    e.Canceled = !isRootCompass && isTargetDocument;
                }
            }
        }

        private void OnClose(object sender, StateChangeEventArgs e)
        {
            //todo implement hide
            //var documents = e.Panes.Select(p => p.DataContext).OfType<PanelBarPanel>().Where(vm => vm.IsDocument).ToList();
            //foreach (var document in documents)
            //{
            //    ((ApplicationShell)this.DataContext).ViewPanesVMs.Remove(document);
            //}
        }
        

        private Dictionary<RadMenuItem, Action> menusActions = new Dictionary<RadMenuItem, Action>();

        public void RegisterMenuItem(string itemName, Action action, string root, IEnumerable<string> menuPath)
        {
            var rootItem = this.FindName(root);
            if(rootItem == null) { return; }

            var rootMenuItem = rootItem as RadMenuItem;
            if(rootMenuItem == null) { return; }

            var currentItem = rootMenuItem;

            foreach(var pathPart in menuPath)
            {
                var nextItemFound = false;
                foreach(var item in currentItem.Items)
                {
                    var menuItem = item as RadMenuItem;
                    if(menuItem != null)
                    {
                        if(menuItem.Header.ToString() == pathPart)
                        {
                            nextItemFound = true;
                            currentItem = menuItem;
                            break;
                        }
                    }
                }

                if(!nextItemFound)
                {
                    var menuItem = new RadMenuItem
                    {
                        Header = pathPart
                    };
                    currentItem.Items.Add(menuItem);
                    currentItem = menuItem;
                }

                //  if (currentItem.Items.)
            }

            var newMenuItem = new RadMenuItem
            {
                Header = itemName
            };

            this.menusActions.Add(newMenuItem, action);

            currentItem.Items.Add(newMenuItem);

            newMenuItem.Click += this.menuItem_Click;

            //  var menuItem = new RadMenuItem();
            //  menuItem.Click += menuItem_Click;
        }

        private void menuItem_Click(object sender, RadRoutedEventArgs e)
        {
            if(this.menusActions.ContainsKey((RadMenuItem) sender)) { this.menusActions[(RadMenuItem) sender].Invoke(); }
        }


        /// <summary>
        ///     TODO
        /// </summary>
        private void SaveLayout()
        {
            /* using (MemoryStream memStream = new MemoryStream())
            {
                MainDocking.SaveLayout(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                StreamReader reader = new StreamReader(memStream);
                string layoutXml = reader.ReadToEnd();
                ApplicationSettings.SetValue("MainLayout", layoutXml);
            }*/

            string xml;
            // Save your layout for example in the isolated storage.
            using(var storage = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                using(var isoStream = storage.OpenFile("MainLayout.xml", FileMode.Create))
                {
                    this.MainDocking.SaveLayout(isoStream);
                    isoStream.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(isoStream);
                    xml = reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        private void LoadLayout()
        {
            //using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForAssembly())
            //{

            //    if (storage.FileExists("MainLayout.xml"))
            //    {
            //        try
            //        {
            //            using (var isoStream = storage.OpenFile("MainLayout.xml", FileMode.Open))
            //            {
            //                MainDocking.LoadLayout(isoStream);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            SystemMessage.SendSystemMessage(SystemMessage.Type.Warning, "Unable to load application layout",
            //                ex.Message, "ApplicationView");
            //        }
            //    }
        }
        #endregion
    }
}