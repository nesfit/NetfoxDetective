// Copyright (c) 2017 Jan Pluskal
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

using GalaSoft.MvvmLight.Command;
using Infragistics.Controls.Maps;

namespace Netfox.AnalyzerAppIdent.Converters
{
    public class NetworkNodeSelectionEventArgsToClusterNodeModelConverter : IEventArgsConverter
    {

        #region Implementation of IEventArgsConverter
        /// <summary>
        /// The method used to convert the EventArgs instance.
        /// </summary>
        /// <param name="value">An instance of EventArgs passed by the
        ///             event that the EventToCommand instance is handling.</param><param name="parameter">An optional parameter used for the conversion. Use
        ///             the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.EventArgsConverterParameter"/> property
        ///             to set this value. This may be null.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public object Convert(object value, object parameter)
        {
            return ((value as NetworkNodeClickEventArgs)?.MouseEventArgs?.Source as NetworkNodeNodeControl)?.Content;
        }
        #endregion
    }
}
