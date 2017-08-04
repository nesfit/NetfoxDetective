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

using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace Netfox.Detective.Views
{
    public class GridExpanderSizeBehavior
    {
        public static DependencyProperty SizeRowsToExpanderStateProperty = DependencyProperty.RegisterAttached("SizeRowsToExpanderState", typeof(bool),
            typeof(GridExpanderSizeBehavior), new FrameworkPropertyMetadata(false, SizeRowsToExpanderStateChanged));

        public static void SetSizeRowsToExpanderState(Grid grid, bool value) { grid.SetValue(SizeRowsToExpanderStateProperty, value); }

        private static void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            var expander = e.OriginalSource as RadExpander;
            var row = Grid.GetRow(expander);
            if(row <= grid.RowDefinitions.Count) { grid.RowDefinitions[row].Height = new GridLength(1.0, GridUnitType.Auto); }
        }

        private static void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            var expander = e.OriginalSource as RadExpander;
            var row = Grid.GetRow(expander);
            if(row <= grid.RowDefinitions.Count) { grid.RowDefinitions[row].Height = new GridLength(1.0, GridUnitType.Star); }
        }

        private static void SizeRowsToExpanderStateChanged(object target, DependencyPropertyChangedEventArgs e)
        {
            var grid = target as Grid;
            if(grid != null)
            {
                if((bool) e.NewValue)
                {
                    grid.AddHandler(RadExpander.ExpandedEvent, new RoutedEventHandler(Expander_Expanded));
                    grid.AddHandler(RadExpander.CollapsedEvent, new RoutedEventHandler(Expander_Collapsed));
                }
                else if((bool) e.OldValue)
                {
                    grid.RemoveHandler(RadExpander.ExpandedEvent, new RoutedEventHandler(Expander_Expanded));
                    grid.RemoveHandler(RadExpander.CollapsedEvent, new RoutedEventHandler(Expander_Collapsed));
                }
            }
        }
    }
}