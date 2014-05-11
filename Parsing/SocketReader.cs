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

namespace CapGUI.Parsing
{
    /// <summary>
    /// Reads the sockets in a block in order to copy them and parse them into plaintext
    /// also manages other socket-reading related functionality
    /// </summary>
    public class SocketReader
    {
        //these are the actual methods used for reading sockets
        #region Main Read Methods & Constructor

        public SocketReader()
        {
        }

        //method to read socket values and place them into parentheses
        public static String readSocket(Block source)
        {
            if (source.flag_hasSocks)
            {
                String plaintext =""; //string that will become the output
                ListBox socket; //listbox to be checked for values
                bool parenCheck = checkLocation(source); //bool to see if socket needs parentheses
                bool specialLoop = checkSpecialLoop(source); //bool to see if socket is in for/wait for loops, which have special outputs

                //adding opening bracket or paren if necessary
                if (source.ToString().Equals("ASSIGNMENT") || source.ToString().Equals("RETURN"))
                {
                    plaintext = " ";
                }
                else if (parenCheck || source.flag_isCustom)
                {
                    plaintext = " ( ";
                }
                else
                {
                    plaintext = "[ ";
                }
                

                //getting all sockets in the block
                List<int> locations = socketFinder(source);

                foreach (int i in locations)
                {
                    socket = socketMole(source, i);
                    
                    //ensuring that the socket contains a block
                    if (socket.Items.Count > 0)
                    {

                        Block infoCube = (Block)socket.Items.ElementAt(0);
                        //handling nested conditions, logic statements, and methods since they have unique styles
                        if (infoCube.flag_transformer)
                        {
                            if (infoCube.flag_isCustom)
                            {
                                plaintext += "method "+ infoCube.metadataList[1] + readSocket(infoCube);
                            }
                            else
                            {
                                plaintext += readSocket(infoCube) + " ";
                            }
                        }
                        //handling blocks with text
                        else if (infoCube.flag_isConstant && infoCube.flag_hasSocks && !infoCube.flag_robotOnly)
                        {
                            plaintext += (infoCube.ToString()).ToLower();
                            int slot = textFinder(infoCube);
                            //general case
                            if (!specialLoop)
                            {
                                //handling string blocks
                                if (infoCube.ToString().Contains("STRING"))
                                {
                                    plaintext += (" \"" + textReader(infoCube, slot) + "\" ").ToLower();
                                }
                                    //handling other, numerical blocks
                                else
                                {
                                    plaintext += (" " + textReader(infoCube, slot) + " ").ToLower();
                                }
                            }
                                //case of for/wait for
                            else
                            {
                                if(source.ToString().Contains("WAIT"))
                                {
                                    return " " + textReader(infoCube, slot) + ";";
                                }
                                else{
                                    return " " + textReader(infoCube, slot);
                                }

                            }
                        }
                            //handling robot functions and constants
                        else if (infoCube.flag_robotOnly && (infoCube.flag_hasSocks || infoCube.ToString().Contains("GETBEARING")))
                        {
                            plaintext += translateRobotFunctions(infoCube);
                        }
                        //handling blocks with comboboxes or non-text constants
                        else if ((infoCube.flag_isRobotConstant || infoCube.flag_isConstant) && !infoCube.flag_hasSocks)
                        {
                            //infinity block case, to be placed into for/wait for
                            if (specialLoop)
                            {
                                return " -1";
                            }
                            //handling any combobox blocks
                            else
                            {
                                ComboBox cb = (ComboBox)infoCube.innerPane.Children.ElementAt(1);
                                //differentiating between the combobox types since bearing/range, direction, and others have different processes
                                if (infoCube.ToString().Contains("BEARING") || source.ToString().Contains("RANGE"))
                                {
                                    plaintext += SocketReader.translateDistanceInputs(cb);
                                }
                                else if (infoCube.ToString().Contains("DIRECTION"))
                                {
                                    plaintext += SocketReader.translateDriveInputs(infoCube);
                                }
                                else
                                {
                                    plaintext += (((TextBlock)cb.SelectionBoxItem).Text + " ").ToLower();
                                }
                            }
                        }
                        //handling variables, since they require 2 strings for identification
                        else if (infoCube.flag_isCustom)
                        {
                            //accounting for using var in front of variable names...except in assign for some reason, which doesn't use it
                            if (source.ToString().Contains("ASSIGN"))
                            {
                                plaintext += infoCube.metadataList[1] + " ";
                            }
                            else
                            {
                                plaintext += "var " + infoCube.metadataList[1] + " ";
                            }
                            plaintext += readSocket(infoCube);
                        }
                        //all other blocks
                        else
                        {
                            plaintext += (infoCube.metadataList[0] + " ").ToLower();
                            plaintext += readSocket(infoCube);
                        }
                        //adding the operator after the first comparator, if necessary
                        if (i == 0 && !source.flag_isCustom)
                        {
                            plaintext += " " + translateComparisons(source) + " ";
                        }
                            //adding the comma needed in parameters
                        else if (source.flag_isCustom && i != locations.ElementAt(locations.Count() - 1))
                        {
                            plaintext += ", ";
                        }
                            //accounting for the equal sign needed in assign
                        else if (i == 2 && locations.Contains(4))
                        {
                            plaintext += " = ";
                        }

                    }
                }

                //adding closing brackets and parens
                if (source.ToString().Equals("ASSIGNMENT") || source.ToString().Equals("RETURN"))
                {
                    //removing the extra space at the end
                    if (plaintext[plaintext.Length-1] != ')')
                    {
                        plaintext = plaintext.Substring(0, plaintext.Length - 1);
                    }
                }
                else if (parenCheck || source.flag_isCustom)
                {
                    plaintext += ")";
                }
                else
                {
                    plaintext += "]";
                }

                return plaintext;
            }
            else
            {
                return "";
            }
        }

        //method to read the contents of a textbox, based on its location within the block
        public static String textReader(Block source, int location)
        {
            String text = "";
            List<System.Windows.UIElement> components = source.innerPane.Children.ToList();

            //handling the differences between a numeric box and standard text
            if (components.ElementAt(location) is NumericTextBox)
            {
                NumericTextBox box = (NumericTextBox)components.ElementAt(location);
                text = box.Text;
            }
            else if (components.ElementAt(location) is TextBox)
            {
                TextBox box = (TextBox)components.ElementAt(location);
                text = box.Text;

            }

            return text;
        }

        #endregion

        //these are methods used to locate blocks and sockets, as well as performing potential transfers
        #region Socket Location and Transfer Methods

        //method to tunnel into the block and return the socket that is at the given position
        public static ListBox socketMole(Block source, int i)
        {
            List<System.Windows.UIElement> components;

            //checking for methods
            if (source.flag_isCustom && source.flag_transformer)
            {
                //checking if in main panel or socket based on whether the line number is present
                if (source.innerPane.Children.Count == 4)
                {
                    StackPanel innards = (StackPanel)source.innerPane.Children.ElementAt(3);
                    components = innards.Children.ToList();
                }
                else
                {
                    StackPanel innards = (StackPanel)source.innerPane.Children.ElementAt(2);
                    components = innards.Children.ToList();
                }
            }
            //checking for assignment blocks and diverting accordingly
            else if (source.ToString().Contains("ASSIGN"))
            {
                StackPanel innards = (StackPanel)source.innerPane.Children.ElementAt(2);
                components = innards.Children.ToList();
            }
            //checking for logic-style blocks
            else if (source.flag_transformer)
            {
                //accounting for situations where the block is already transformed
                if (source.innerPane.Children.ElementAt(0) is StackPanel)
                {
                    StackPanel innards = (StackPanel)source.innerPane.Children.ElementAt(0);
                    components = innards.Children.ToList();
                }
                else
                {
                    i++;
                    components = source.innerPane.Children.ToList();
                }
            
            }
            else
            {
                components = source.innerPane.Children.ToList();
            }

            //getting the socket to be returned
            SocketDragDropTarget SDDT = (SocketDragDropTarget)components.ElementAt(i);
            ListBox listBox = (ListBox)SDDT.Content;
                
            return listBox;
        }

        //method to find and return the indices of the sockets in a block
        public static List<int> socketFinder(Block source)
        {
            List<int> socketList = new List<int>();
            List<System.Windows.UIElement> components;

            //checking for logic-style blocks
            if (!(source.innerPane.Children.ElementAt(0) is TextBlock))
            {
                StackPanel innards = (StackPanel)source.innerPane.Children.ElementAt(0);
                components = innards.Children.ToList();
            }
                //checking for assignment blocks and diverting accordingly
            else if (source.ToString().Contains("ASSIGN") && source.innerPane.Children.Count > 2)
            {
                StackPanel innards = (StackPanel)source.innerPane.Children.ElementAt(2);
                components = innards.Children.ToList();
            }
            //checking for method blocks
            else if (source.flag_isCustom && source.flag_transformer)
            {
                //checking if in main panel or socket, based on the presence of the line number
                if (source.innerPane.Children.Count == 4)
                {
                    StackPanel innards = (StackPanel)source.innerPane.Children.ElementAt(3);
                    components = innards.Children.ToList();
                }
                else
                {
                    //accounting for a method with no parameters
                    if (source.innerPane.Children.ElementAt(2) is TextBlock)
                    {
                        components = new List<UIElement>();
                    }
                    else
                    {
                        StackPanel innards = (StackPanel)source.innerPane.Children.ElementAt(2);
                        components = innards.Children.ToList();
                    }
                }
            }
                //all other cases
            else
            {
                components = source.innerPane.Children.ToList();
            }
            //cycling through and grabbing any actual sockets in the block
                for (int i = 0; i < components.Count; i++)
                {
                    if (components.ElementAt(i) is SocketDragDropTarget)
                    {
                        socketList.Add(i);
                    }
                }
            
            return socketList;
        }

        //method to find the textbox in a block
        public static int textFinder(Block source)
        {
            int location = -1;
            List<System.Windows.UIElement> components = source.innerPane.Children.ToList();

            //cycling through the block components to find the locations of text or combo boxes
            for (int j = 0; j < components.Count; j++)
                if (components.ElementAt(j) is NumericTextBox || components.ElementAt(j) is TextBox || components.ElementAt(j) is ComboBox)
                {
                    location = j;
                }
            return location;
        }

        //method to transfer textbox data
        public static Block textTransfer(Block source, Block destination)
        {
            int location = textFinder(source);
            if (location != -1)
            {
                //accounting for transferring comboboxes
                if (source.ToString().Contains("DIRECTION"))
                {
                    
                    ComboBox x = (ComboBox)source.innerPane.Children.ElementAt(location);
                    int selection = x.SelectedIndex;

                    //copying the combobox and selection options since stupid silverlight apparently has no simple method for it that doesn't crash after opening the new box...
                    List<TextBlock> itemCopies = new List<TextBlock>();
                    for (int i = 0; i < x.Items.Count; i++)
                    {
                        x.SelectedIndex = i;
                        TextBlock temp = new TextBlock();
                        temp.Text = ((TextBlock)x.SelectionBoxItem).Text;
                        itemCopies.Add(temp);
                    }

                    //creating and inserting new block's combobox
                    ComboBox y = new ComboBox();
                    y.ItemsSource = itemCopies;
                    y.SelectedIndex = selection;//x.SelectedIndex;
                    destination.innerPane.Children.Add(y);
                }
                else
                {
                    String text = textReader(source, location);

                    int dstLocation = textFinder(destination);
                    TextBox x = (TextBox)destination.innerPane.Children.ElementAt(dstLocation);
                    x.Text = text;
                }

            }
            return destination;
        }

        //method to find a particular variable or method in the tree
        public static int checkCustoms(ReadOnlyObservableCollection<Block> blockList, Block toFind)
        {
            int numFound = 0;

            foreach (Block block in blockList)
            {
                //checking the block's sockets for the target
                if (block.flag_hasSocks)
                {
                    numFound += findSocketedCustoms(block, toFind);
                }
                //accounting for any possibility of the metadatalist count being less than 2
                try
                {
                    if (block.metadataList[1] == toFind.metadataList[1])
                    {
                        numFound++;
                    }
                }
                catch (Exception) { }
            }

            return numFound;
        }

        //method to find variables or methods in sockets
        public static int findSocketedCustoms(Block platform, Block toFind)
        {
            int numFound = 0;
            List<int> socketLocs = socketFinder(platform);
            ListBox socket;
            //cycling through all sockets in the block to look for target
            foreach (int locale in socketLocs)
            {
                socket = socketMole(platform, locale);
                //ensuring that the socket isn't empty
                if (socket.Items.Count > 0)
                {
                    Block tempBlock = (Block)socket.Items.ElementAt(0);
                    //recursively going deeper to check more sockets
                    if (tempBlock.flag_hasSocks)
                    {
                        numFound += findSocketedCustoms(tempBlock, toFind);
                    }

                    if (tempBlock.metadataList.Count > 1 && tempBlock.metadataList[1].Equals(toFind.metadataList[1]))
                    {
                        numFound++;
                    }
                }
            }

            return numFound;
        }

        //method to return a list of sockets in the destination block, based on the destination listbox
        private static List<ListBox> getDestinationBlockSockets(ListBox destination)
        {
            List<ListBox> socketList = new List<ListBox>();//list of sockets found
            SocketDragDropTarget SDDT = (SocketDragDropTarget)destination.Parent;//destination listbox's socket target
            StackPanel panel = (StackPanel)SDDT.Parent; //destination block's stackpanel
            List<System.Windows.UIElement> components = panel.Children.ToList(); //children of destination block

            //cycling through the block's children to find a socket and adding it to the list if so
            foreach (System.Windows.UIElement element in components)
            {
                if(element is SocketDragDropTarget)
                {
                    ListBox b = ((SocketDragDropTarget)element).Content as ListBox;
                    if (b.ToString().Contains("ListBox"))
                    {
                        socketList.Add(b);
                    }
                }
            }
            return socketList;
        }

        #endregion

        //these are methods used to check types, contents, and other properties of sockets
        #region Observation Methods

        //method to check for blocks with empty sockets
        public static int checkOpenSocks(ReadOnlyObservableCollection<Block> blockList)
        {
            int numFound = 0;

            //cycling through every block to be checked
            foreach (Block block in blockList)
            {
                if (block.flag_hasSocks)
                {
                    numFound += findOpenSockets(block);
                }
            }

            return numFound;
        }

        //method to check for empty sockets in blocks
        private static int findOpenSockets(Block platform)
        {
            int numFound = 0;
            List<int> socketLocs = socketFinder(platform);

            //seeing if any sockets exist in the block
            if (socketLocs.Count == 0)
            {
                int textLocs = textFinder(platform);
                //accounting for text sockets
                if (textLocs != -1)
                {
                    if (textReader(platform, textLocs) == "")
                    {
                        numFound++;
                    }

                }
            }

            ListBox socket;
            //cycling through each found socket, to be checked
            foreach (int locale in socketLocs)
            {
                socket = socketMole(platform, locale);
                //only continue if sockets are found
                if (socket.Items.Count > 0)
                {
                    Block tempBlock = (Block)socket.Items.ElementAt(0);
                    //recursively continue deeper into sockets
                    if (tempBlock.flag_hasSocks)
                    {
                        numFound += findOpenSockets(tempBlock);
                    }
                }
                else
                {
                    numFound++;
                }
            }

            return numFound;
        }

        //checks if the block is directly in the editor or if its in a socket
        private static bool checkLocation(Block source)
        {
            bool topLevel = false;

            //socket must be in editor if it has no parent
            if (source.Parent == null)
            {
                topLevel = true;
            }
                //some robot functions fail to trigger properly, this corrects that
            else if (source.flag_robotOnly)
            {
                topLevel = true;
            }
            return topLevel;
        }

        //method that compares types, based on their basic declarations
        private static bool checkTypes(Block occupant, Block immigrant)
        {

            if(occupant.returnType.Equals(immigrant.returnType))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        //method to facilitate typechecking
        public static bool socketCompatable(ListBox destination, Block immigrant)
        {

            Block block1;
            List<ListBox> socketList = getDestinationBlockSockets(destination);

            //ensuring that there are at least 2 sockets to check
            //the socket must match flag is also true
            if (socketList.Count > 1 && getParentBlock(destination).flag_socketsMustMatch)
            {
                ListBox socket1 = socketList.ElementAt(0);
                ListBox socket2 = socketList.ElementAt(1);
                if (socket1.Items.Count > 0)
                {
                    block1 = (Block)socket1.Items.ElementAt(0);
                    return checkTypes(block1, immigrant);
                }
                else if (socket2.Items.Count > 0)
                {
                    block1 = (Block)socket2.Items.ElementAt(0);
                    return checkTypes(block1, immigrant);
                }
                    //always return true if both sockets are empty
                else
                {
                    return true;
                }
            }
                //always return true if there is only 1 socket
            else
            {
                return true;
            }
        }

        //method to make sure the return type matches the return block type
        public static bool returnMustMatch(ListBox destination, Block immigrant)
        {
            Block block1 = getParentBlock(destination);
            if (immigrant.returnType.Equals(block1.returnType))
                return true;
            return false;
        }

        //returns the string of the dropTarget's parent block
        public static string checkParentBlock(ListBox dropTarget)
        {
            SocketDragDropTarget SDDT = (SocketDragDropTarget)dropTarget.Parent; //destination listbox's socket drop target
            StackPanel SP = (StackPanel)SDDT.Parent; //destination listbox's main stack panel
            Grid G;//declaring the main grid
            //checking if the destination listbox is a transformed block
            if (SP.Parent is StackPanel)
            {
                StackPanel SP2 = (StackPanel)SP.Parent;//transformed block's outer stackpanel
                G = (Grid)SP2.Parent; //transformed block's primary grid
            }
            else
            {
                G = (Grid)SP.Parent; //destination listbox's primary grid
            }
            Canvas C = (Canvas)G.Parent; //destination listbox's main canvas
            Border B = (Border)C.Parent; //destination listbox's outer border
            return B.Parent.ToString(); //returning the destination listbox's actual parent block name
        }

        //return the block of the dropTarget's parent
        public static Block getParentBlock(ListBox dropTarget)
        {
            SocketDragDropTarget SDDT = (SocketDragDropTarget)dropTarget.Parent;//destination listbox's socket drop target
            StackPanel SP = (StackPanel)SDDT.Parent;//destination listbox's main stack panel
            //checking if the destination listbox is a transformed block
            if (SP.Parent is StackPanel)
                SP = (StackPanel)SP.Parent;//transformed block's outer stackpanel
            Grid G = (Grid)SP.Parent;//destination listbox's primary grid
            Canvas C = (Canvas)G.Parent;//destination listbox's main canvas
            Border B = (Border)C.Parent;//destination listbox's outer border
            return (Block)B.Parent;//returning the destination listbox's actual parent block
        }

        //checks if block is a loop which will require no parens, i.e. for/wait for
        private static bool checkSpecialLoop(Block b)
        {
            if (b.ToString().Contains("FOR"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //method to check for variables in the tree, used for tab management
        public static void ReplaceMethodBlocks(ObservableCollection<Block> blockList, Block newMethodBlock)
        {
            foreach (Block block in blockList)
            {
                if (block.flag_hasSocks)
                {
                    findSocketedMethods(block, newMethodBlock);
                }
            }
        }

        //method to check sockets for variables
        private static void findSocketedMethods(Block platform, Block newMethodBlock)
        {
            List<int> socketLocs = socketFinder(platform);
            ListBox socket;
            //cycling through sockets to search
            foreach (int locale in socketLocs)
            {
                socket = socketMole(platform, locale);
                if (socket.Items.Count > 0)
                {
                    Block tempBlock = (Block)socket.Items.ElementAt(0);
                    //recursive call to go as deep into socket as possible
                    if (tempBlock.flag_hasSocks && !tempBlock.Text.Equals("METHOD"))
                    {
                        findSocketedMethods(tempBlock, newMethodBlock);
                    }
                    else if (tempBlock.metadataList.Count >= 2 && tempBlock.metadataList[1].Equals(newMethodBlock.metadataList[1])) //bearing and infinite blocks need the metadatalistcheck
                    {
                        ListBox socketBox = tempBlock.Parent as ListBox;
                        SocketDragDropTarget SDDT = socketBox.Parent as SocketDragDropTarget;
                        SDDT.removeItem(socketBox);
                        SDDT.ResizeAndAdd(socketBox, newMethodBlock.cloneSelf(true));
                        
                    }
                    else if (tempBlock.flag_hasSocks) //method in a method check?
                    {
                        findSocketedMethods(tempBlock, newMethodBlock);
                    }
                }
            }
        }

        #endregion

        //these are methods that convert block formats into their objective G counterparts, mostly used for robot functions
        #region Translation Methods

        //method to translate robot functions into code
        public static string translateRobotFunctions(Block b)
        {
            string functName = b.ToString();

            switch (functName)
            {
                case "DRIVE":
                    return "method drive " + readSocket(b);
                    //break;
                case "DRIVEDISTANCE":
                    return "method driveDistance " + readSocket(b);
                   // break;
                case "TURN":
                    return "method turn " + readSocket(b);
                    //break;
                case "TURNDEGREES":
                    return "method turnAngle " + readSocket(b);
                    //break;
                case "TURNTOBEARING":
                    return "method turnToBearing " + readSocket(b);
                    //break;
                case "CHECKRANGE":
                    return "method getSonars " + readSocket(b);
                    //break;
                case "GETBEARING":
                    return "method getBearing ()";
                    //break;
                case "STOP":
                    return "method stop ()";
                    //break;
                default:
                    Debug.WriteLine("ERROR: METHOD NOT RECOGNIZED");
                    return "";
                    //break;
            }

        }

        //method to convert comparison names into the proper symbols
        private static string translateComparisons(Block b)
        {
            if (b.ToString().Contains("AND") || b.ToString().Contains("OR"))
            {
                return b.ToString().ToLower();
            }
            switch (b.ToString())
            {
                case "COMPARISON-greater":
                    return ">";
                case "COMPARISON-less":
                    return "<";
                case "COMPARISON-greaterequal":
                    return ">=";
                case "COMPARISON-lessequal":
                    return "<=";
                case "COMPARISON-equal":
                    return "==";
                case "COMPARISON-notequal":
                    return "!=";
                case "ASSIGNMENT":
                    return "=";
                default:
                    return "error";
            }
        }

        //checks and translates distance method parameters
        private static string translateDistanceInputs(ComboBox cb)
        {
            string contents = ((TextBlock)cb.SelectionBoxItem).Text;
            switch (contents)
            {
                case "FRONT":
                    return "int 1 ";
                   // break;
                case "REAR":
                    return "int 4 ";
                    //break;
                case "LEFT":
                    return "int 5 ";
                    //break;
                case "RIGHT":
                    return "int 3 ";
                case "NORTH":
                    return "int 0 ";
                    //break;
                case "SOUTH":
                    return "int 180 ";
                    //break;
                case "EAST":
                    return "int 90 ";
                    //break;
                case "WEST":
                    return "int 270 ";
                    //break;
                default:
                    Debug.WriteLine("ERROR: UNKNOWN DIRECTION");
                    return "";
                    //break;
            }
            /*if(contents.Contains("FRONT"))
            {
                return "int 1 ";
            }
            else if(contents.Contains("REAR"))
            {
                return "int 2 ";
            }
            else if(contents.Contains("LEFT"))
            {
                return "int 3 ";
            }
            else
            {
                return "int 4 ";
            }*/
        }

        //checks and translates movement block parameters
        private static string translateDriveInputs(Block b)
        {

            ComboBox cb = (ComboBox)b.innerPane.Children.ElementAt(1);

            string contents = ((TextBlock)cb.SelectionBoxItem).Text;
                if (contents.Contains("FORWARD"))
                {
                    return "string \"f\" ";
                }
                else if(contents.Contains("BACK"))
                {
                    return "string \"b\" ";
                }
                if (contents.Contains("LEFT"))
                {
                    return "string \"l\" ";
                }
                else
                {
                    return "string \"r\" ";
                }
            
        }

        #endregion

    }
}
