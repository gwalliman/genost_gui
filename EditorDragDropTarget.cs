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
    /**
     * Represents the editor as a target for dropping blocks
     */
    public class EditorDragDropTarget : ListBoxDragDropTarget
    {

        private BlockTreeList tree = new BlockTreeList();
        private bool move = false; //TRUE when we are dragging something
        private int fromIndex = 0; //Holds the old index of the item being dragged
        
        //Called when we start to drag a block
        protected override void OnItemDragStarting(ItemDragEventArgs eventArgs)
        {
            //Debug.WriteLine("Editor drag start");
            SelectionCollection selectionCollection = eventArgs.Data as SelectionCollection;
            foreach (Selection selection in selectionCollection)
            {
                if (selection.Item.GetType().Equals(typeof(Block)))
                {
                    //Don't allow an END block to be dragged
                    if (((Block)selection.Item).Text.Contains("END"))
                    {
                        eventArgs.Cancel = true;
                        eventArgs.Handled = true; 
                    }
                    else
                    {
                        //If we are not holding a socket, get index and set move to TRUE
                        if (!MainPage.communicate.socket)
                        {
                            move = true;
                            fromIndex = ((Block)selection.Item).index;
                        }

                        //Throw to parent
                        base.OnItemDragStarting(eventArgs);
                    }
                }
            }
        }

        //Called when we have released a block
        protected override void OnItemDroppedOnTarget(ItemDragEventArgs args)
        {
            ListBox listBox = args.DragSource as ListBox;
            SelectionCollection selectionCollection = args.Data as SelectionCollection;

            //Search through the data until we find the block we are dragging
            foreach (Selection selection in selectionCollection)
            {
                if (selection.Item.GetType().Equals(typeof(Block)))
                {
                    move = false;

                    //If we are over the trash when we release, delete the block from the tree
                    if (MainPage.communicate.trash)
                    {
                        MainPage.communicate.trash = false;
                        listBox.ItemsSource = tree.Delete(((Block)selection.Item).index);
                    }
                }
            }
            try
            {
                //Call the standard drop target handler
                base.OnItemDroppedOnTarget(args);
                //Refresh the tree
                listBox.ItemsSource = tree.ListRefresh();
                //Refresh the listbox (list of selectable items)
                Refresh(listBox);
            }
            catch (Exception exe)
            {
            }

        }

        /**
         * I think this is called when we drop a block
         */
        protected override void OnDropOverride(Microsoft.Windows.DragEventArgs args)
        {
            //Ping for activity
            MainPage.ping();
            if ((args.AllowedEffects & Microsoft.Windows.DragDropEffects.Link) == Microsoft.Windows.DragDropEffects.Link || (args.AllowedEffects & Microsoft.Windows.DragDropEffects.Move) == Microsoft.Windows.DragDropEffects.Move)
            {
                //changed
                //gets the data format which is a ItemDragEventArgs
                object data = args.Data.GetData(args.Data.GetFormats()[0]);

                //changed
                //cast from generic object to ItemDragEventArgs and add to SelectionCollection
                ItemDragEventArgs itemDragEventArgs = data as ItemDragEventArgs;
                SelectionCollection selectionCollection = itemDragEventArgs.Data as SelectionCollection;

                //changed
                //get the target & source listbox from DragEventArgs
                ListBox dropTarget = GetDropTarget(args);
                //ListBox src = (ListBox)itemDragEventArgs.DragSource;

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

                    if (index != null)
                    {
                        if (args.Effects == Microsoft.Windows.DragDropEffects.Move && itemDragEventArgs != null && !itemDragEventArgs.DataRemovedFromDragSource)
                        {
                            itemDragEventArgs.RemoveDataFromDragSource();
                        }

                        //major change place at top of the listbox to act as a stack for undo
                        //needs to have its own method
                        foreach (Selection selection in selectionCollection)
                        {

                            if (selection.Item.GetType().Equals(typeof(Block)))
                            {

                                //socket was added to the editor
                                if (MainPage.communicate.socket)
                                {
                                    MainPage.communicate.editor = true; //let the socket know that it will need to be replaced
                                    MainPage.communicate.socket = false; //trying to fix the error

                                    //catching constant into constant in a first level block
                                    Block selected = selection.Item as Block;
                                    if (selected.flag_isConstant || selected.flag_isRobotConstant)
                                    {
                                        dropTarget = itemDragEventArgs.DragSource as ListBox;
                                        dropTarget.Items.Add(selected);
                                    }
                                }
                                else if (move) //moving in the editor window
                                {
                                    move = false;
                                    //Debug.WriteLine("move from " + fromIndex + " to " + index.Value);
                                    if (index.Value < dropTarget.Items.Count && (((Block)dropTarget.Items[index.Value]).Text.Equals("ELSE") || ((Block)dropTarget.Items[index.Value]).Text.Equals("ELSEIF")))
                                    {
                                        dropTarget.ItemsSource = tree.Move(fromIndex, fromIndex);
                                    }
                                    else
                                    {
                                        dropTarget.ItemsSource = tree.Move(fromIndex, index.Value);
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine(((Block)selection.Item).typeID);
                                    Block copyBlock = MainPage.createProgramStructureBlock(((Block)selection.Item).typeID);

                                    if (copyBlock == null)
                                    {
                                        if (!((Block)selection.Item).Text.Equals("END"))
                                            copyBlock = ((Block)selection.Item).cloneSelf(true);
                                        //else if (((Block)selection.Item).Text.Equals("METHOD"))
                                        //    copyBlock = ((Block)selection.Item).cloneSelf();
                                        else
                                            copyBlock = MainPage.createReservedBlock(((Block)selection.Item).Text);
                                    }
                                    if (copyBlock.checkType("STATEMENT"))
                                    {
                                        if (copyBlock.flag_requiresEndIf)
                                        {
                                            //check if it is an else or else if and make sure that count and index are not 0 or null to preform operatoins

                                            if (copyBlock.flag_followsIfOnly && dropTarget.Items.Count > 0 && index.Value > 0)
                                            {
                                                //check previous index for end if block
                                                //add if it exist
                                                if (((Block)dropTarget.Items[index.Value - 1]).Text.Equals("ENDIF"))
                                                {
                                                    if (copyBlock.Text.Equals("ELSE"))
                                                    {
                                                        //check to see if there is already an else
                                                        if (index.Value < dropTarget.Items.Count && (((Block)dropTarget.Items[index.Value]).Text.Equals("ELSE") || ((Block)dropTarget.Items[index.Value]).Text.Equals("ELSEIF"))) { }
                                                        else
                                                        {
                                                            Block endBlock = MainPage.createReservedBlock("ENDELSE");//.cloneSelf();
                                                            dropTarget.ItemsSource = tree.AddWithChild(copyBlock, endBlock, index.Value);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Block endBlock = MainPage.createReservedBlock("ENDIF");//.cloneSelf();
                                                        dropTarget.ItemsSource = tree.AddWithChild(copyBlock, endBlock, index.Value);
                                                    }
                                                }
                                            }
                                            else if (!copyBlock.flag_followsIfOnly)
                                            {
                                                //doesn't allow if to be placed inbetween an if and else-if or else
                                                if (index.Value < dropTarget.Items.Count && (((Block)dropTarget.Items[index.Value]).Text.Equals("ELSEIF") || ((Block)dropTarget.Items[index.Value]).Text.Equals("ELSE"))) { }
                                                else
                                                {
                                                    Block endBlock = MainPage.createReservedBlock("ENDIF");//.cloneSelf();
                                                    dropTarget.ItemsSource = tree.AddWithChild(copyBlock, endBlock, index.Value);
                                                }
                                            }
                                        }
                                        else if (index.Value < dropTarget.Items.Count && (((Block)dropTarget.Items[index.Value]).Text.Equals("ELSE") || ((Block)dropTarget.Items[index.Value]).Text.Equals("ELSEIF"))) { }
                                        else if (copyBlock.flag_requiresEndLoop)
                                        {
                                            Block endBlock = MainPage.createReservedBlock("ENDLOOP");//.cloneSelf();
                                            dropTarget.ItemsSource = tree.AddWithChild(copyBlock, endBlock, index.Value);
                                        }
                                        else
                                        {
                                            dropTarget.ItemsSource = tree.Add(copyBlock, index.Value);
                                        }
                                    }
                                }
                            }
                        }
                        Refresh(dropTarget);
                        //change was made
                        MainPage.communicate.changeCodeColorStatus();
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
        }

        private void Refresh(ListBox listBox)
        {
            foreach(Block b in listBox.Items)
            {
                b.setLine(listBox.Items.IndexOf(b));
            }
        }

        #region Socket and Method Functions

        public ReadOnlyObservableCollection<Block> getTreeList()
        {
            return tree.GetBlocks();
        }

        //returns a list of blocks with the new updated block
        public ObservableCollection<Block> switchBlocks(Block newBlock, bool Name)
        {
           return tree.UpdateBlock(newBlock, Name);
        }

        public void switchBlocks(ListBox dropTarget, Block newBlock, bool Name)
        {
            dropTarget.ItemsSource = tree.UpdateBlock(newBlock, Name);
        }

        #endregion

        #region Used To Load

        //Method that clears old tree
        public void clearTree(ListBox dropTarget)
        {
            dropTarget.ItemsSource = tree.clearList();
        }

        public void addNodeToTree(Block block, int index)
        {
            tree.AddNoReturn(block, index);
        }
        
        public void addNodeToTree(Block block, Block endBlock, int index)
        {
            tree.AddWithChildNoReturn(block, endBlock, index);
        }

        public void setTree(ListBox dropTarget)
        {
            dropTarget.ItemsSource = tree.ReturnList();
        }
        #endregion
    }
}
