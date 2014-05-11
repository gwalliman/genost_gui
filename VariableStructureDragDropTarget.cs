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
    public class VariableStructureDragDropTarget : ListBoxDragDropTarget
    {

        private ObservableCollection<Block> list;
        //private ListBox listBox = new ListBox();

        protected override void OnItemDragCompleted(ItemDragEventArgs args)
        {
            base.OnItemDragCompleted(args);

            ListBox listBox = args.DragSource as ListBox;
            var testingList = listBox.ItemsSource;
   
            if (testingList.GetType().Equals(typeof(ObservableCollection<Block>)))
            {
                ObservableCollection<Block> blockList = testingList as ObservableCollection<Block>;
                if (listBox != null)
                {
                    listBox.ItemsSource = null;
                    blockList.Clear();
                    foreach (Block b in list)
                    {
                        blockList.Add(b);
                    }
                    listBox.ItemsSource = blockList;
                }
            }   
        }

        protected override void OnItemDragStarting(ItemDragEventArgs eventArgs)
        {
            SelectionCollection selectionCollection = eventArgs.Data as SelectionCollection;
            
            foreach (Selection selection in selectionCollection)
            {
                if (selection.Item.GetType().Equals(typeof(Block)))
                {
                    ListBox listBox = eventArgs.DragSource as ListBox;
                    var testingList = listBox.ItemsSource;
                    if (testingList.GetType().Equals(typeof(ObservableCollection<Block>)))
                    {
                        ObservableCollection<Block> blockList = testingList as ObservableCollection<Block>;
                        list = new ObservableCollection<Block>();
                        foreach (Block b in blockList)
                        {
                            list.Add(b);
                        }
                    }
                    base.OnItemDragStarting(eventArgs);     
                }
            }
        }
    }
}
