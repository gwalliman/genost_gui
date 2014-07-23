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
using CapGUI.Parsing;

namespace CapGUI
{
    public class SocketDragDropTarget : ListBoxDragDropTarget
    {
        #region Globals and Constants
        public string socketType { get; set; }      //define what blocks are allowed in socket from xml
        public bool isConstant { get; set; }        //define if constants are allowed in socket
        public bool isCondition { get; set; }       //define if condition is allowed in socket
        #endregion

        #region Overwritten Methods
        /*
         * Event method that is called when a socket item starts to move.
         * Sets the socket communication to true, which allows other DDT's to know socket is being moved
         * Calls parent method
         */
        protected override void OnItemDragStarting(ItemDragEventArgs eventArgs)
        {
            Debug.WriteLine("socket on item drag start");
            MainPage.communicate.socket = true;
            base.OnItemDragStarting(eventArgs);
        }

        /*
         * Event method that is called when a socket item is dropped on destination.
         * Preforms functions based on the communication class.
         * Set the communication flag for socket to false (handled).
         * Calls parent method
         */
        protected override void OnItemDroppedOnTarget(ItemDragEventArgs args)
        {
            Debug.WriteLine("socket on item drop");
            ListBox listBox = args.DragSource as ListBox;

            SelectionCollection selectionCollection = args.Data as SelectionCollection;
            foreach (Selection selection in selectionCollection)
            {
                if (selection.Item.GetType().Equals(typeof(Block)))
                {
                    //create the block based on selection block
                    Block copyBlock = MainPage.createProgramStructureBlock(((Block)selection.Item).typeID);
                    if (copyBlock == null)
                    {
                        if (!((Block)selection.Item).Text.Contains("END"))
                            copyBlock = ((Block)selection.Item).cloneSelf(true);
                        else
                            copyBlock = MainPage.createReservedBlock(((Block)selection.Item).Text);
                    }

                    if (MainPage.communicate.trash) //socket moved to trash
                    {
                        MainPage.communicate.trash = false; //let trash know that socket finished move
                        listBox.Background.Opacity = 1; //show empty socket
                        listBox.BorderBrush.Opacity = 1; //show empty socket
                    }
                    else if (MainPage.communicate.editor) //socket moved to editor (removed communicate.socket to see if communication error is fixed)
                    {
                        //copyBlock goes here
                        if (listBox.Items.Count == 0)
                        {
                            Debigulate(listBox);
                            if (socketType.Contains(copyBlock.Text))
                                ResizeAndAdd(listBox, copyBlock);
                            else if (isConstant && copyBlock.flag_isConstant)
                                ResizeAndAdd(listBox, copyBlock);
                            else if (isCondition && copyBlock.flag_isCondition)
                                ResizeAndAdd(listBox, copyBlock);
                        }

                        MainPage.communicate.editor = false; //let editor know that that socket was added and move finished
                    }
                    else //socket moved else where...need to have a check to see if it was added
                    {
                        if (listBox.Items.Count == 0)
                        {
                            listBox.Background.Opacity = 1; //show empty socket
                            listBox.BorderBrush.Opacity = 1; //show empty socket
                        }
                    }
                }
            }
            base.OnItemDroppedOnTarget(args);
            MainPage.communicate.socket = false; //save that moveing socket is finished

            //change was made
            MainPage.communicate.changeCodeColorStatus();

            //returning the block to normal size, if neccessary
            Debigulate(listBox);
        }

        /*
         * Event method that is called when an item is dropped into socket.
         * Checks to see if block can be added to socket and prefroms functions based on those checks.
         * Set the communication flag for socket to false (handled).
         */
        protected override void OnDropOverride(Microsoft.Windows.DragEventArgs args)
        {
            MainPage.ping();
            Debug.WriteLine("socket drop");
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
                //get the target & source listbox from DragEventArgs
                ListBox dropTarget = GetDropTarget(args);

                //Copy from parent method
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
                                //creat copy block of the selection item
                                Block copyBlock = MainPage.createProgramStructureBlock(((Block)selection.Item).typeID);
                                if (copyBlock == null || (copyBlock != null && !copyBlock.type.Equals("STATEMENT"))) //check to make sure that you are not adding a STATEMENT to SOCKET
                                {
                                    Block selected = (Block)selection.Item;
                                    //check to make sure that you are not dragging into self
                                    dropTarget = preventOuroboros(itemDragEventArgs, dropTarget, selected, selected);

                                    if (copyBlock == null)
                                    {

                                        if (!((Block)selection.Item).Text.Equals("END"))
                                            copyBlock = (selected).cloneSelf(true);
                                        else
                                            copyBlock = MainPage.createReservedBlock(selected.Text);
                                    }

                                    //check to make sure that if there are more than 1 socket that they match
                                    if (SocketReader.socketCompatable(dropTarget, copyBlock))
                                    {
                                        //alter comboboxes if needed
                                        if (checkRestrictions(dropTarget, copyBlock))
                                        {
                                            if (copyBlock.ToString().Equals("DIRECTION"))
                                            {
                                                directionalCombos(dropTarget, copyBlock);
                                            }

                                            //flag to check if an element did move
                                            //used for cloneSockets check
                                            bool flag_DidAdd = false;

                                            //Add item to socket if allowed
                                            if (socketType.Contains(copyBlock.Text) || (copyBlock.returnType != null && socketType.Contains(copyBlock.returnType)))
                                            {
                                                ResizeAndAdd(dropTarget, copyBlock);
                                                flag_DidAdd = true;
                                            }
                                            else if (isConstant && copyBlock.flag_isConstant)
                                            {
                                                ResizeAndAdd(dropTarget, copyBlock);
                                                flag_DidAdd = true;
                                            }
                                            else if (isCondition && copyBlock.flag_isCondition)
                                            {
                                                ResizeAndAdd(dropTarget, copyBlock);
                                                flag_DidAdd = true;
                                            }

                                            //clone sockets if item was added
                                            if (copyBlock.flag_hasSocks && flag_DidAdd)
                                            {
                                                copyBlock = cloneSockets(selected, copyBlock);
                                                //change was made
                                                MainPage.communicate.changeCodeColorStatus();
                                            }
                                            if (!flag_DidAdd)
                                            {
                                                ErrorMessage(1);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ErrorMessage(2);
                                    }
                                }
                                else
                                {
                                    ErrorMessage(3);
                                }
                            }
                        }
                        //socket is no longer dragging
                        MainPage.communicate.socket = false; //try to fix half of the communication error
                        
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

        //method to display errors encountered when adding blocks to editor
        private static void ErrorMessage(int code)
        {
            MessageBoxButton buttons = MessageBoxButton.OK; //only allow OK and X buttons
            string message = "";
            switch (code)
            {
                    //block in unacceptable socket
                case 1:
                    message = "That block cannot be placed here";
                    break;
                    //block put into mismatched socket
                case 2:
                    message = "Block type doesn't match adjoining socket";
                    break;
                    //block put into wrong socket
                case 3:
                    message = "Wrong block type for socket";
                    break;
                    //unexpected issue
                default:
                    message = "Miscellaneous error";
                    break;
            }
            MessageBoxResult result = MessageBox.Show(message, "Error", buttons); //display message window
            MainPage.errorMessage(message);
        }
        #endregion

        #region Add, Change and Resize
        //method to ensure block returns to correct size if needed
        private void Debigulate(ListBox listBox)
        {
            //making sure shrinkage only occurs the block leaves the pool of expanded blocks
            if (listBox.Items.Count < 1 && listBox.Height > 40)
            {
                while (listBox.Height > 40)
                {
                    AdjustBlock(listBox, false);
                }
            }
        }

        //method to change combo boxes in directional blocks
        private static void directionalCombos(ListBox dropTarget, Block copyBlock)
        {
            ComboBox cb = (ComboBox)copyBlock.innerPane.Children.ElementAt(2);

            //if turn, remove front, rear, backwards and forwards
            if (SocketReader.checkParentBlock(dropTarget).Contains("TURN"))
            {
                Debug.WriteLine(copyBlock.innerPane.Children.ElementAt(2));
                cb.Items.RemoveAt(3);
                cb.Items.RemoveAt(2);
                cb.Items.RemoveAt(1);
                cb.Items.RemoveAt(0);
                cb.SelectedItem = cb.Items.ElementAt(0);
            }
            //if drive, remove front,rear, left and right
            else if (SocketReader.checkParentBlock(dropTarget).Contains("DRIVE"))
            {
                Debug.WriteLine(copyBlock.innerPane.Children.ElementAt(2));
                cb.Items.RemoveAt(5);
                cb.Items.RemoveAt(4);
                cb.Items.RemoveAt(3);
                cb.Items.RemoveAt(2);
            }
                //if distance, remove backwards and forwards
            else if (SocketReader.checkParentBlock(dropTarget).Contains("RANGE"))
            {
                Debug.WriteLine(copyBlock.innerPane.Children.ElementAt(2));
                cb.Items.RemoveAt(1);
                cb.Items.RemoveAt(0);
                cb.SelectedIndex = 0;
            }
        }
        
        public void ResizeAndAdd(ListBox listBox, Block block)
        {

            try //try to add and resize (attempting to catch the drag into self error)
            {
                block.LayoutRoot.Width = listBox.Width * .94;//note: any higher percentage causes blocks to but cut off in assignment sockets
                block.LayoutRoot.Height = listBox.Height * .7;
                block.innerPane.Children.Remove(block.line);

                if (block.flag_hasSocks || block.ToString().Contains("BEARING"))
                {
                    AdjustSockets(block);

                    block.innerPane.Width = block.LayoutRoot.Width - 3;

                    if (block.flag_transformer)
                    {
                        block = LogicTransform(listBox, block);
                    }
                    block.innerPane.Height = block.LayoutRoot.Height - 1;
                }
                //Note: the following is antiquated...I think...So I'm leaving it here just in case something goes haywire.(Obviously if that is the case, remove the catch for bearing above)
                /*else if (block.ToString().Contains("BEARING") || block.ToString().Contains("DIRECTION"))
                {
                    block.LayoutRoot.Width = listBox.Width * .75;
                    block.LayoutRoot.Height = listBox.Height * .51;
                }*/
                if (listBox.Items.Count < 1)
                {
                    AddItem(listBox, block);

                    //Removing the box & border of the socket
                    listBox.Background.Opacity = 0;
                    if (listBox.BorderBrush == null)
                    {
                        listBox.BorderBrush = new SolidColorBrush(Colors.Purple);
                    }
                    listBox.BorderBrush.Opacity = 0;
                }
            }
            catch (Exception e) //print error
            {
                Debug.WriteLine(e);
            }
        }

        //method to adjust sockets so they fit properly in socketted blocks
        private void AdjustSockets(Block block)
        {
            //looping through each component so it can be cast to its object to change properties
            List<System.Windows.UIElement> components = block.innerPane.Children.ToList();
            for (int i = 0; i < components.Count; i++)
            {
                //casting sockets into specific objects so their widths can be changed
                //catching method sockets
                if (components.ElementAt(i) is StackPanel && block.flag_isCustom)
                {
                    StackPanel y = (StackPanel)components.ElementAt(i);
                    List<System.Windows.UIElement> methodSockets = y.Children.ToList();
                    foreach (UIElement UI in methodSockets)
                    {
                        SocketDragDropTarget SDDT = (SocketDragDropTarget)UI;
                        SDDT.Width = block.LayoutRoot.Width * .63;
                        ListBox inner = (ListBox)SDDT.Content;
                        inner.Width = block.LayoutRoot.Width * .63;
                        components.RemoveAt(i);
                        components.Insert(i, SDDT);
                    }
                }
                //catching standard sockets
                else if (components.ElementAt(i) is SocketDragDropTarget)
                {
                    SocketDragDropTarget SDDT = (SocketDragDropTarget)components.ElementAt(i);
                    SDDT.Width = block.LayoutRoot.Width * .96;
                    ListBox inner = (ListBox)SDDT.Content;
                    inner.Width = block.LayoutRoot.Width * .96;
                    components.RemoveAt(i);
                    components.Insert(i, SDDT);
                }
                //ensuring text boxes are caught as well
                else if (components.ElementAt(i) is NumericTextBox || components.ElementAt(i) is TextBox)
                {
                    TextBox x = (TextBox)components.ElementAt(i);
                    x.Width = block.LayoutRoot.Width * .4;
                    components.RemoveAt(i);
                    components.Insert(i, x);
                }
            }
        }

        //method to convert a logic block so as to work with nesting
        private Block LogicTransform(ListBox listBox, Block block)
        {
            //routing accordingly if block is a method
            if (block.flag_isCustom)
            {
                return MethodTransform(listBox, block);
            }

            //Changing the socket's block to account for stacking style
            for (int i = 0; i < 3; i++)
            {
                    AdjustBlock(listBox, true);
            }
            block.LayoutRoot.Height *= 3.5;

            //creating and populating a stacked block
            List<System.Windows.UIElement> components = block.innerPane.Children.ToList();
            block.innerPane.Children.Clear();//clearing the block so the layout can be changed easier

            StackPanel stackBox = new StackPanel();
            stackBox.Width = block.LayoutRoot.Width;
            foreach (System.Windows.UIElement element in components)
            {
                //casting sockets into specific objects so their widths can be changed
                if (element is SocketDragDropTarget)
                {
                    SocketDragDropTarget x = (SocketDragDropTarget)element;
                    x.Width = stackBox.Width - 2;
                    ListBox inner = (ListBox)x.Content;
                    inner.Width = stackBox.Width - 2;
                    stackBox.Children.Add(x);

                }
                else
                    stackBox.Children.Add(element);
            }

            //style to eliminate excess padding
            Style style = new Style(typeof(StackPanel));
            style.Setters.Add(new Setter(StackPanel.MarginProperty, new Thickness(-1)));
            stackBox.Style = style;

            //setting up elements in order to hide whitespace
            SolidColorBrush tempBrush = new SolidColorBrush(Colors.Purple);
            stackBox.Background = tempBrush;
            stackBox.Background.Opacity = 0;

            block.innerPane.Children.Add(stackBox);

            return block;
        }

        //method to provide incremental adjustments for method blocks
        private Block MethodTransform(ListBox listBox, Block block)
        {
            StackPanel conan = (StackPanel)block.innerPane.Children.ElementAt(2);
           
            //shrinking block & socket sizes to readjust for newest parameter count
            block.LayoutRoot.Height = 40 * .7;
            while (listBox.Height > 40)
            {
                Debigulate(listBox);
            }

            //individually running through each parameter and expanding as needed
            for (int i = 0; i < conan.Children.Count; i++)
            {
                SocketDragDropTarget needle = (SocketDragDropTarget)conan.Children.ElementAt(i);
                needle.Height *= .75;
                ListBox zula = (ListBox)needle.Content;
                zula.Height *= .75;
                if (i > 0)
                {
                    AdjustBlock(listBox, true);
                    block.LayoutRoot.Height += 31;
                }
            }
            return block;
        }

        //a noble method that embiggens the smallest socket
        //definitely a perfectly cromulent way to do this
        private void AdjustBlock(ListBox listBox, bool embiggen)
        {
            //amount to expand or deflate blocks by
            int expansionFactor = 31;

            SocketDragDropTarget socketDDT;//the listbox's socket target
            StackPanel stackPanel;//the listbox's main stack panel
            Canvas canvas;//the listbox's main canvas
            Grid grid;//the listbox's primary grid
            Border border;//the listbox's outer border
            Block block;//the listbox's block

            //expanding the block
            if (embiggen)
            {
                listBox.Height += expansionFactor;
                socketDDT = (SocketDragDropTarget)listBox.Parent;
                socketDDT.Height += expansionFactor;
                stackPanel = (StackPanel)socketDDT.Parent;

                //checking if socket is within a transformed block
                if (stackPanel.Parent is StackPanel)
                {
                    StackPanel webby = (StackPanel)stackPanel.Parent;//transformed block's inner stack panel
                    webby.Height += expansionFactor;
                    grid = (Grid)webby.Parent;
                }
                else
                {
                    grid = (Grid)stackPanel.Parent;
                }

                grid.Height += expansionFactor;
                canvas = (Canvas)grid.Parent;
                canvas.Height += expansionFactor;
            }
            //shrinking the block
            else
            {
                listBox.Height -= expansionFactor;
                socketDDT = (SocketDragDropTarget)listBox.Parent;
                socketDDT.Height -= expansionFactor;
                stackPanel = (StackPanel)socketDDT.Parent;

                //checking if socket is within a transformed block
                if (stackPanel.Parent is StackPanel)
                {
                    StackPanel webby = (StackPanel)stackPanel.Parent;//transformed block's inner stack panel
                    webby.Height -= expansionFactor;
                    grid = (Grid)webby.Parent;
                }
                else
                {
                    grid = (Grid)stackPanel.Parent;
                }
                grid.Height -= expansionFactor;
                canvas = (Canvas)grid.Parent;
                canvas.Height -= expansionFactor;
            }

            //checking to see if recursion is necessary
            border = (Border)canvas.Parent;
            block = (Block)border.Parent;
            if (block.Parent != null)
            {
                //recursively calling the method for block's parent listbox
                AdjustBlock((ListBox)block.Parent, embiggen);
            }
        }
        #endregion

        #region Clone and Restrict
        //copies the socket contents of a block on moving
        private Block cloneSockets(Block source, Block destination)
        {
            //checks if the sockets are textboxes and routes accordingly
            if ((destination.flag_isConstant || destination.flag_isRobotConstant) && !destination.flag_hasSocks)
            {
                destination = SocketReader.textTransfer(source, destination);
            }
            else
            {
                List<int> socketList = SocketReader.socketFinder(source);

                //cycling through all sockets
                foreach (int location in socketList)
                {
                    ListBox socket = SocketReader.socketMole(destination, location);
                    ListBox sourceSock = SocketReader.socketMole(source, location);
                    //making sure sockets aren't empty
                    if (sourceSock.Items.Count > 0)
                    {
                        Block hubert = (Block)sourceSock.Items.ElementAt(0);
                        Block cubert = hubert.cloneSelf(hubert.flag_hasSocks);

                        //add the block to the socket
                        ResizeAndAdd(socket, cubert);

                        //recursion to catch nested sockets
                        cubert = cloneSockets(hubert, cubert);
                    }
                }
            }

            return destination;
        }
       
        /*
         * Check socket restrictions, will not allow blocks to be added if they are restricted
         * Check based on return type of blocks
         * Checks method returns as well
         */
        private bool checkRestrictions(ListBox listBox, Block child)
        {
            Block parent = SocketReader.getParentBlock(listBox);    //get parent block
            string returnType = child.returnType;                   //get return type
            
            //If parent is the return block make sure the returnType matches the return block type
            if (parent.Text.Equals("RETURN"))
            {
                if (SocketReader.returnMustMatch(listBox, child))
                    return true;
                else
                    return false;
            }
            else
            {
                //Check to make sure block is allowed in socket based on return type
                switch (returnType)
                {
                    case "INT":
                        return !parent.flag_intDisabled;
                    case "STRING":
                        return !parent.flag_stringDisabled;
                    case "BOOL":
                        return !parent.flag_booleanDisabled;
                    default:
                        return true;
                }
            }
        }

        //method to prevent a block from going into one of its own sockets
        private ListBox preventOuroboros(ItemDragEventArgs itemDragEventArgs, ListBox dropTarget, Block immigrant, Block destination)
        {
            //testing dragging into self and catching it if it happens
            List<int> socks = SocketReader.socketFinder(destination);
            ListBox test;
            ListBox sourceBox = itemDragEventArgs.DragSource as ListBox;

            //handling constants
            if (immigrant.flag_isConstant || immigrant.flag_isRobotConstant)
            {
                //checking constants, accounting for their 1 lvl lower problem
                try
                {
                    Block source = SocketReader.getParentBlock(sourceBox);
                    Block dst = SocketReader.getParentBlock(dropTarget);
                    List<int> socketLocs = SocketReader.socketFinder(dst);
                    foreach (int loc in socketLocs)
                    {
                        ListBox targ = SocketReader.socketMole(dst, loc);
                        if (targ.Items.Count > 0)
                        {
                            //checking if the target is the current block's location
                             if (((Block)dropTarget.Items.ElementAt(0)) == source)
                            {
                                isConstant = immigrant.flag_isConstant;//for some reason this gets reset during the process, so setting it back
                                dropTarget = sourceBox;//changing the target to the block's current position, dropping it back in place
                            }
                        }
                    }
                }
                catch (Exception) { }
                
                //clearing the target so the block can be added
                if (dropTarget.Items.Count > 0)
                {
                    if(((Block)dropTarget.Items.ElementAt(0)) == immigrant)
                    dropTarget.Items.Clear();
                }
            }
            //checking the block's socket to ensure that it isn't being added to itself
            foreach (int loc in socks)
            {
                test = SocketReader.socketMole(destination, loc);
                //resetting the drop zone if true and shrinking the block down
                if (test == dropTarget)
                {
                    dropTarget = sourceBox;
                    Debigulate(dropTarget);
                    isCondition = immigrant.flag_isCondition;
                }
                //recursively calling, to ensure that the block isn't being added to a socket in a block within its own socket
                if (test.Items.Count > 0)
                {
                    dropTarget = preventOuroboros(itemDragEventArgs, dropTarget, immigrant, (Block)test.Items.ElementAt(0));
                }
            }
            return dropTarget;
        }
        #endregion

        /*
         * Remove the item in the socket
         */
        public void removeItem(ListBox listBox)
        {
            RemoveItemAtIndex(listBox, 0);
        }


    }
}
