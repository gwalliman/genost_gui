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
using System.Windows.Controls.Internals;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace CapGUI
{

    public class TrashDragDropTarget : ListBoxDragDropTarget
    {
        public bool AllowAdd { get; set; }

        protected override void OnItemDragStarting(ItemDragEventArgs eventArgs) 
        { 
            eventArgs.Cancel = true; 
            eventArgs.Handled = true; 
        }

        protected override void OnDropOverride(Microsoft.Windows.DragEventArgs args)
        {
            //Debug.WriteLine("AllowAdd: " + AllowAdd);
            //Debug.WriteLine("trash");
            MainPage.communicate.trash = true;
            if (AllowAdd)
            {
                if ((args.AllowedEffects & Microsoft.Windows.DragDropEffects.Link) == Microsoft.Windows.DragDropEffects.Link
                   || (args.AllowedEffects & Microsoft.Windows.DragDropEffects.Move) == Microsoft.Windows.DragDropEffects.Move)
                {
                    //changed
                    //gets the data format which is a ItemDragEventArgs
                    object data = args.Data.GetData(args.Data.GetFormats()[0]);

                    //changed
                    //cast from generic object to ItemDragEventArgs and add to SelectionCollection
                    ItemDragEventArgs itemDragEventArgs = data as ItemDragEventArgs;
                    SelectionCollection selectionCollection = itemDragEventArgs.Data as SelectionCollection;

                    //changed
                    //get the target listbox from DragEventArgs
                    ListBox dropTarget = GetDropTarget(args);

                    if (dropTarget != null && selectionCollection.All(selection => CanAddItem(dropTarget, selection.Item)))
                    {
                        if ((args.Effects & Microsoft.Windows.DragDropEffects.Move) == Microsoft.Windows.DragDropEffects.Move)
                        {
                            args.Effects = Microsoft.Windows.DragDropEffects.Move;
                        }
                        else
                        {
                            args.Effects = Microsoft.Windows.DragDropEffects.Link;
                        }

                        int? index = GetDropTargetInsertionIndex(dropTarget, args);
                        //Debug.WriteLine("?index: " + index.Value);
                        if (index != null)
                        {
                            if (args.Effects == Microsoft.Windows.DragDropEffects.Move && itemDragEventArgs != null && !itemDragEventArgs.DataRemovedFromDragSource)
                            {
                                itemDragEventArgs.RemoveDataFromDragSource();
                            }

                            //major change place at top of the listbox to act as a stack for undo
                            foreach (Selection selection in selectionCollection)
                            {
                                InsertItem(dropTarget, 0, selection.Item);
                            }
                        }
                    }
                    else
                    {
                        args.Effects = Microsoft.Windows.DragDropEffects.None;
                    }

                    if (args.Effects != args.AllowedEffects)
                    {
                        args.Handled = true;
                    }
                }
                AllowAdd = false;
                MainPage.communicate.changeCodeColorStatus();
            }
        }
    }
}
