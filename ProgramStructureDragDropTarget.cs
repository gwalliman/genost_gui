using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;

namespace CapGUI
{
    public class ProgramStructureDragDropTarget : ListBoxDragDropTarget
    {
        protected override void OnItemDragStarting(ItemDragEventArgs eventArgs)
        {
            SelectionCollection selectionCollection = eventArgs.Data as SelectionCollection;
            foreach (Selection selection in selectionCollection)
            {

                if (selection.Item.GetType().Equals(typeof(Block)))
                {
                    if (((Block)selection.Item).flag_isPackage)
                    {
                        eventArgs.Cancel = true;
                        eventArgs.Handled = true;
                    }
                    else
                    {
                        base.OnItemDragStarting(eventArgs);
                        ListBox listBox = eventArgs.DragSource as ListBox;
                        var testingList = listBox.ItemsSource;
                        if (testingList.GetType().Equals(typeof(ObservableCollection<Block>)))
                        {
                            ObservableCollection<Block> list = testingList as ObservableCollection<Block>;
                            listBox.ItemsSource = null;
                            var itemsToRemove = list.Where(item => !item.flag_isPackage).ToArray();
                            foreach (var item in itemsToRemove)
                            {
                                list.Remove(item);
                            }
                            listBox.ItemsSource = list;

                            listBox.ScrollIntoView(listBox.Items.ElementAt(0));
                        }
                    }
                }
            }
        }
    }
}
