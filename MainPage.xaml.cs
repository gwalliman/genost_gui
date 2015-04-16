using System;
using System.Collections.Generic; //SelectionCollection
using System.Linq; //used for generic calls
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;
using System.Reflection;
using System.Diagnostics; //debug
using System.Windows.Controls.Internals; //toolkit
using System.Collections.ObjectModel; //ObservableCollection
using System.ComponentModel; //ICollectionView
using System.Windows.Data; //CollectionViewSource
using System.Windows.Controls.Primitives; //Popup
using CapGUI.Parsing;
using CapGUI.xml;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Linq;
using System.Windows.Browser;
using System.Threading;
using System.Runtime.Serialization.Json;

namespace CapGUI
{
    public partial class MainPage : UserControl
    {
        #region Globals and Constants
        private List<Panel> colPanels = new List<Panel>();

        private ObservableCollection<Block> programStructureList;
        private ObservableCollection<Block> robotFunctionsList;
        public static ObservableCollection<Block> variableList;
        public static ObservableCollection<Block> methodList;

        //All blocks + packages
        public static List<List<Block>> allBlockList;
        private List<String> packageNameList;
        public static List<Block> reservedBlocks;

        //N
        public static Block draggedItem = null;
        private PopupInterface varPop;
        private PopupInterface methodPop;
        private PopupInterface lessonPick;
        private PopupInterface savePop;

        //Color palette
        Color varColor = Color.FromArgb(255, 255, 174, 201);
        Color robotFunctionColor = Color.FromArgb(255, 255, 201, 14);
        Color programStructureColor = Color.FromArgb(255, 153, 217, 234);

        private string etrVarName = "Enter Variable Name";
        private string etrMethodName = "Enter Method Name";

        //used for method tabs to access the items
        public static List<TabItem> tabList; //contains a list of all the tabs

        public static DragDropTargetCommunication communicate;

        public static List<String> nameList; //contains a list of all user created names used for uniqueness
        public static List<EditorDragDropTarget> editorLists; //contains a list of all the editors
        private int LastSelectedTab = -1;

        private bool robotRunning = false;  //determines if robot is currently running and code should be stopped or executed on button click
        private bool loadLibrary = false;   //
        private IEnumerable<XElement> xmlDoc = null;
        public ObservableCollection<String> lessons_master = null; //lesson array

        private Dictionary<int, string> lessonDic;
        private string mazeID;

        private Login lw;
        private static string username;
        private static string password;
        private bool freeMode;
        private string currentLessonId;
        private string currentLessonImage;
        private bool doneLoading = false;
        private bool waitingOnLessonLoad = false;
        
        #endregion

        #region Main
        private static MainPage instance;
        public static MainPage Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainPage();
                }
                return instance;
            }
        }

        /**
         * Load and set up everything
         */
        private MainPage()
        {
            //Load the components, put them into place.
            InitializeComponent();
            
            //Initialize the list of lessons
            lessonDic = new Dictionary<int, string>();
            
            //Initialize various lists for program structures
            programStructureList = new ObservableCollection<Block>();
            robotFunctionsList = new ObservableCollection<Block>();
            variableList = new ObservableCollection<Block>();
            methodList = new ObservableCollection<Block>();

            //Initialize the list of tabs (methods)
            tabList = new List<TabItem>();

            //Create the global state tracker
            communicate = new DragDropTargetCommunication();

            nameList = new List<String>();
            
            //We use the editorLists to keep track of the different main editors across tabs
            editorLists = new List<EditorDragDropTarget>();

            //Add the Main canvas to the editorList set
            editorLists.Add(editorDragDrop);

            //Reads in blocks from the local xmlDoc. This can probably be removed since we never use the local xmlDoc.
            //readBlockAPI(false, xmlDoc);
            
            //Set ItemsSource of ListBox to desired Lists
            //This graphically populates the various canvas sections with the blocks
            blockPalette.ItemsSource = programStructureList;
            robotPalette.ItemsSource = robotFunctionsList;
            variablePalette.ItemsSource = variableList;
            methodPalette.ItemsSource = methodList;
            
            //Handle clicking on blocks in the editor canvas
            editorPalette.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Handle_EditorMouseDown), true);
            editorPalette.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(Handle_EditorMouseUp), true);

            //Add handlers to open / close the block palettes
            blockPalette.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Handle_ProgramMouseDown), true);
            robotPalette.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Handle_RobotMouseDown), true);

            //Add click handler to variable palette.
            variablePalette.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Handle_VarMethMouseDown), true);

            //Use variable click handler for method palette since they are the same
            methodPalette.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Handle_VarMethMouseDown), true);

            //Waiting until the page is loaded before launching login method call
            this.Loaded += MainPage_Loaded;
        }

        /**
         * Load code blocks in from a specific source
         * If an xmlFile is provided, we load from there.
         * If not, we load from a local XML file
         */
        private bool readBlockAPI(bool loadFromServer, IEnumerable<XElement> xmlFile)
        {
            BlockAPIReader r = null;
            //If we have loaded a doc from the server, we use that as our source
            //If not, we use the default source, i.e. a local doc
            if (loadFromServer)
            {
                r = new BlockAPIReader(xmlFile);
            }
            else
            {
                r = new BlockAPIReader();
            }

            //Create lists of all blocks
            allBlockList = r.readBlockDefinitions();

            //Get the package blocks and reserved blocks
            packageNameList = r.getPackageNames();
            reservedBlocks = r.getReservedBlocks();

            //Set current lesson ID
            mazeID = currentLessonId;

            //Add package marker blocks to the Program Structures palette
            for (int i = 0; i < allBlockList.Count; i++)
            {
                
                //Does the package contain any non-reserved blocks?
                bool containsNonReserved = false;
                //Does the package contain any blocks that are part of the Program Structures panel?
                bool isPartOfProgramPanel = true;

                //Iterate through each block in the package
                /**
                 * TODO: Refactor required. Currently this code will not add ANY blocks if there is at least one block in the package that is not in the Program Structures panel
                 * This is not a big deal right now due to how we define our packages but this should still be fixed
                 */
                foreach (Block b in allBlockList[i])
                {
                    //If the block is non-reserved
                    if (!b.flag_programOnly)
                    {
                        containsNonReserved = true;
                        //If the block does not go in the Program Structures panel
                        if (b.flag_robotOnly)
                        {
                            isPartOfProgramPanel = false;
                        }
                        break;
                    }
                }

                //If the package contains at least one nonreserved block, and if it does not contain any robot blocks
                //Add a marker block for the package to the Program Structures panel
                if (containsNonReserved && isPartOfProgramPanel)
                {
                    //If it does, create a marker block for that package
                    Block packageBlock = new Block(packageNameList[i], ((Block)allBlockList[i][0]).blockColor);
                    packageBlock.LayoutRoot.Width = blockPalette.Width - 30;
                    packageBlock.flag_isPackage = true;
                    programStructureList.Add(packageBlock);
                }
                //If the package contains at least one nonreserved block, and if it contains at least one robot blocks
                //Add a marker block for the package to the Robot panel
                else if (containsNonReserved && !isPartOfProgramPanel)
                {
                    Block packageBlock = new Block(packageNameList[i], ((Block)allBlockList[i][0]).blockColor);
                    packageBlock.LayoutRoot.Width = blockPalette.Width - 30;
                    packageBlock.flag_isPackage = true;
                    robotFunctionsList.Add(packageBlock);
                }
            }

            //Unclear as to why we even return anything, as the returned value is never used anywhere
            if (allBlockList != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        //Here there be draggin' (handlers)
        #region Handlers

        //When clicking a variable, don't allow it to be trashed.
        private void Handle_VarMethMouseDown(object sender, MouseButtonEventArgs e)
        {
            trashDragDrop.AllowAdd = false; // can't move these blocks to trash
            ListBox listBox = sender as ListBox;
            //Set the variable as selected
            draggedItem = (Block)listBox.SelectedItem;
        }

        //When clicking in the editor canvas, if we have clicked a block, allow it to be placed in the trash and get its index
        private void Handle_EditorMouseDown(object sender, MouseEventArgs args)
        {
            trashDragDrop.AllowAdd = false;
            ListBox listBox = sender as ListBox;
            if (listBox.SelectedItem != null)
            {
                trashDragDrop.AllowAdd = true;
                ((Block)listBox.SelectedItem).index = listBox.SelectedIndex;
            }
        }

        //When releasing the mouse in the editor canvas, reset the indices of the editor items
        private void Handle_EditorMouseUp(object sender, MouseEventArgs args)
        {
            for (int i = 0; i < this.editorPalette.Items.Count; i++)
            {
                ((Block)this.editorPalette.Items[i]).index = (i);
            }
        }

        //Mouse down handler for Program Structures block
        //Handle opening a package
        private void Handle_ProgramMouseDown(object sender, MouseEventArgs args)
        {
            trashDragDrop.AllowAdd = false; // can't move these blocks to trash
            ListBox listbox = sender as ListBox;
            int index = listbox.SelectedIndex;

            //If we have clicked on a valid block
            if (listbox.SelectedItem != null && listbox.SelectedItem.GetType().Equals(typeof(Block)))
            {
                Block temp = listbox.SelectedItem as Block;

                //If the block we clicked on is a Package block AND (The clicked block is either the last one in the list, OR the block below the clicked block is a package)
                //In other words, if we have clicked on a closed Package block
                if (temp.flag_isPackage && (index == listbox.Items.Count - 1 || ((Block)listbox.Items[index + 1]).flag_isPackage))
                {
                    //Loop through each package
                    foreach (String s in packageNameList)
                    {
                        //Find the package in the packageNameList
                        if (s.Equals(temp.Text))
                        {
                            List<Block> list = allBlockList[packageNameList.IndexOf(s)];
                            listbox.ItemsSource = null;
                            //Insert the blocks that belong to the package beneath the package block
                            for (int i = list.Count - 1; i >= 0; i--)
                            {
                                /**
                                 * TODO: Figure out wtf is going on here
                                 * We go to all the trouble to set up tempBlock but then don't use it
                                 * Am I missing something?
                                 */
                                Block tempBlock = list[i] as Block;
                                tempBlock.Margin = new Thickness(50, 0, 0, 0);
                                string tempName = tempBlock.returnName();
                                if (tempName != null)
                                    tempBlock.fore.Text = tempName;
                                tempBlock.LayoutRoot.Width = blockPalette.Width - 80;
                                programStructureList.Insert(index + 1, list[i] as Block);
                            }
                            
                            //Update the listbox
                            listbox.ItemsSource = programStructureList;
                        }
                    }
                }
                //Else if the block we clicked on is a package block AND (The clicked block's index is NOT the last one in the list, OR the block below the clicked block is NOT a package)
                //In other words, if we have clicked on an open package block
                else if (temp.flag_isPackage && (index != listbox.Items.Count - 1 || !((Block)listbox.Items[index + 1]).flag_isPackage))
                {
                    int startPoint = index;
                    int endPoint = startPoint + 1;

                    //Walk from the package block that was clicked forward over all blocks inside that package
                    while (endPoint < listbox.Items.Count - 1 && !((Block)listbox.Items[endPoint]).flag_isPackage)
                    {
                        endPoint++;
                    }

                    //If our last block is not the final block in the list, back off one
                    if (endPoint != listbox.Items.Count - 1)
                    {
                        endPoint--;
                    }
                    //Else if the currently pointed to block is a package, back off one
                    else if (((Block)listbox.Items[endPoint]).flag_isPackage)
                    {
                        endPoint--;
                    }

                    listbox.ItemsSource = null;

                    //Remove the package blocks from the list
                    for (int i = endPoint; i > startPoint; i--)
                    {
                        programStructureList.RemoveAt(i);
                    }
                    //Update the listbox
                    listbox.ItemsSource = programStructureList;
                }
            }
        }

        //Mouse down handler for Robot Functions block
        //Handle opening a package
        private void Handle_RobotMouseDown(object sender, MouseEventArgs args)
        {
            trashDragDrop.AllowAdd = false; // can't move these blocks to trash
            ListBox listbox = sender as ListBox;
            int index = listbox.SelectedIndex;

            //If we have clicked on a valid block
            if (listbox.SelectedItem != null && listbox.SelectedItem.GetType().Equals(typeof(Block)))
            {
                Block temp = listbox.SelectedItem as Block;

                //If the block we clicked on is a Package block AND (The clicked block is either the last one in the list, OR the block below the clicked block is a package)
                //In other words, if we have clicked on a closed Package block
                if (temp.flag_isPackage && (index == listbox.Items.Count - 1 || ((Block)listbox.Items[index + 1]).flag_isPackage))
                {
                    //Loop through each package
                    foreach (String s in packageNameList)
                    {
                        //Find the package in the packageNameList
                        if (s.Equals(temp.Text))
                        {
                            List<Block> list = allBlockList[packageNameList.IndexOf(s)];
                            listbox.ItemsSource = null;

                            //Insert the blocks that belong to the package beneath the package block
                            for (int i = list.Count - 1; i >= 0; i--)
                            {
                                /**
                                 * TODO: Figure out wtf is going on here
                                 * We go to all the trouble to set up tempBlock but then don't use it
                                 * Am I missing something?
                                 */
                                Block tempBlock = list[i] as Block;
                                tempBlock.Margin = new Thickness(50, 0, 0, 0);
                                string tempName = tempBlock.returnName();
                                if (tempName != null)
                                    tempBlock.fore.Text = tempName;
                                tempBlock.LayoutRoot.Width = blockPalette.Width - 80;
                                robotFunctionsList.Insert(index + 1, list[i] as Block);
                            }

                            //Update the listbox
                            listbox.ItemsSource = robotFunctionsList;
                        }
                    }
                }
                //Else if the block we clicked on is a package block AND (The clicked block's index is NOT the last one in the list, OR the block below the clicked block is NOT a package)
                //In other words, if we have clicked on an open package block
                else if (temp.flag_isPackage && (index != listbox.Items.Count - 1 || !((Block)listbox.Items[index + 1]).flag_isPackage))
                {
                    int startPoint = index;
                    int endPoint = startPoint + 1;

                    //Walk from the package block that was clicked forward over all blocks inside that package
                    while (endPoint < listbox.Items.Count - 1 && !((Block)listbox.Items[endPoint]).flag_isPackage)
                    {
                        endPoint++;
                    }

                    //If our last block is not the final block in the list, back off one
                    if (endPoint != listbox.Items.Count - 1)
                    {
                        endPoint--;
                    }
                    //Else if the currently pointed to block is a package, back off one
                    else if (((Block)listbox.Items[endPoint]).flag_isPackage)
                    {
                        endPoint--;
                    }

                    listbox.ItemsSource = null;
                    //Remove the package blocks from the list
                    for (int i = endPoint; i > startPoint; i--)
                    {
                        robotFunctionsList.RemoveAt(i);
                    }
                    //Update the listbox
                    listbox.ItemsSource = robotFunctionsList;
                }
            }
        }

        /*
         * Selection Change Event
         * Save the new selected index to LastSelectedTab
         * This is called by someting in the XAML
         */
        private void Handle_TabSelectedChange(object sender, SelectionChangedEventArgs args)
        {
            TabControl tab = sender as TabControl;

            //If we didn't click on the current tab
            if (this.LastSelectedTab != tab.SelectedIndex)
            {
                this.LastSelectedTab = tab.SelectedIndex;
                
                //If we have editorLists (we always should)
                if (editorLists != null)
                {
                    //Handle the name for use with the Clear Button
                    string tabName = editorLists[LastSelectedTab].Name;
                    tabName = tabName.Remove(tabName.Length - 8);   //change TabPage name to method name, remove 'DragDrop'
                    if (tabName.Equals("editor"))
                    {
                        tabName = "Main";
                    }
                    clearBtn.Content = "CLEAR " + tabName;
                }
            }
        }

        #endregion

        #region Variable Creation

        //Click handler for Variable Creation
        private void createVariableBtn_Click(object sender, RoutedEventArgs e)
        {
            //Set up popup
            varPop = new PopupInterface(Color.FromArgb(255, 255, 174, 201), etrVarName, 8, -90, "VARIABLE");
            varPop.PopupComboBox.Items.RemoveAt(3);
            variableGrid.Children.Add(varPop);
            Grid.SetColumn(varPop, 2);
            varPop.OkAddBtn.Click += new RoutedEventHandler(variableAdd_Click);
            varPop.CreatePopup();
        }

        //Click handler for the Add button in the Create Variable popup
        private void variableAdd_Click(object sender, RoutedEventArgs e)
        {
            string text = varPop.PopupTextBox.Text;
            bool fail = false;

            //Check to make sure we've selected a valid variable type and that we've given it a name
            if (varPop.PopupComboBox.SelectedItem != null && (!text.Equals(etrVarName) && !text.Equals("")))
            {
                //Make sure we don't already have a variable with that name
                foreach (string s in nameList)
                {
                    if (s.Equals(text))
                    {
                        fail = true;
                        break;
                    }
                }
                //If this is a new, unique variable
                if (!fail)
                {
                    //Add variable's name to the list
                    nameList.Add(text);
                    //Close the popup
                    varPop.MenuPopup.IsOpen = false;
                    //Create the variable
                    createVariable(text, (varPop.PopupComboBox.SelectionBoxItem as TextBlock).Text);
                }
            }
        }

        //used to create a new variable
        public void createVariable(string name, string type)
        {
            Block newVariable = createReservedBlock("VARIABLE");
            newVariable.type = "variable";
            newVariable.metadataList[0] = type;
            newVariable.returnType = newVariable.metadataList[0];
            newVariable.metadataList[1] = name;
            newVariable = newVariable.cloneSelf(true);
            newVariable.LayoutRoot.Width = blockPalette.Width - 30;
            variableList.Add(newVariable);
        }

        //Click handler for variable delete button
        private void deleteVariableBtn_Click(object sender, RoutedEventArgs e)
        {
            varPop = new PopupInterface(Color.FromArgb(255, 255, 174, 201), etrVarName, 8, -90, "VARIABLE");
            variableGrid.Children.Add(varPop);
            Grid.SetColumn(varPop, 2);

            //If we have selected a variable
            if (draggedItem != null && draggedItem.type == "variable")
            {
                //This opens a confirmation delete popup
                variablePalette.ItemsSource = varPop.DeletePopup(variableList, variableList[variableList.IndexOf(draggedItem)]);
            }
            else
            {
                variableGrid.Children.Remove(varPop);
            }
        }

        #endregion

        #region Method Creation

        //Click handler for create method button
        private void createMethodBtn_Click(object sender, RoutedEventArgs e)
        {
            //Create popup
            methodPop = new PopupInterface(Color.FromArgb(255, 153, 207, 126), etrMethodName, 8, -90, "METHOD");
            methodGrid.Children.Add(methodPop);
            Grid.SetColumn(methodPop, 2);
            methodPop.OkAddBtn.Click += new RoutedEventHandler(methodAdd_Click);
            methodPop.CreatePopup();
        }

        //Click handler for "OK" button in Method Add popup
        private void methodAdd_Click(object sender, RoutedEventArgs e)
        {
            string text = methodPop.PopupTextBox.Text;
            bool fail = false;

            //Ensure that a method return type has been selected, and that its name is not the default text, or is not empty
            if (methodPop.PopupComboBox.SelectedItem != null && (!text.Equals(etrMethodName) && !text.Equals("")))
            {
                //Ensure that a method with that name does not already exist
                foreach (string s in nameList)
                {
                    if (s.Equals(text))
                    {
                        fail = true;
                        break;
                    }
                }
                //If we've made it to here without any problems, add the method
                if (!fail)
                {
                    nameList.Add(text);
                    methodPop.MenuPopup.IsOpen = false;
                    createMethod(methodPop.PopupTextBox.Text, (methodPop.PopupComboBox.SelectionBoxItem as TextBlock).Text);
                }
            }
        }

        //used to create method
        public void createMethod(string name, string type)
        {
            Block newMethod = createReservedBlock("METHOD");
            newMethod.type = "STATEMENT/PLUGIN";
            newMethod.metadataList[1] = name;
            newMethod.returnType = type;
            newMethod = newMethod.cloneSelf(false);
            newMethod.LayoutRoot.Width = blockPalette.Width - 30;
            methodList.Add(newMethod);
            createMethodTab(name, type);
        }

        //create the new method tab and add to the editorTabControl and add new tab Editor Drag Drop Target to editorList
        private void createMethodTab(string name, string type)
        {
            TabItem thisItem = new TabItem();
            thisItem.MaxWidth = 75;
            thisItem.Header = name;

            thisItem.FontWeight = FontWeights.Bold;
            thisItem.FontSize = 18;
            thisItem.Background = new SolidColorBrush(Colors.LightGray);
            
            //tab page and name creation
            TabPage tabPage = new TabPage();
            string boxName = name + "Palette";
            string dragDropName = name + "DragDrop";
            string itemName = name + "Tab";
             
            //name assigning
            thisItem.Name = itemName;
            tabPage.tempDragDrop.Name = dragDropName;
            tabPage.tempListBox.Name = boxName;
            
            //content assigning
            thisItem.Content = tabPage;
            editorTabControl.Items.Add(thisItem);
            tabList.Add(thisItem);
            editorLists.Add(tabPage.tempDragDrop);    //add tab editorDragDrop to dictionary
            
            //retrieve the index of the comboBox and set the new tab return type to that index
            foreach (ComboBoxItem comboBox in tabPage.returnType.Items)
            {
                
                string compareType = (comboBox.Content as TextBlock).Text;  //get the name of current box
                if (compareType.Equals(type))
                {
                    tabPage.returnType.SelectedIndex = tabPage.returnType.Items.IndexOf(comboBox);  //set the index
                }
            }
            
            //Create a return block for the method
            tabPage.CreateMethodReturnBlock();
        }

        //Delete handler for method
        private void deleteMethodBtn_Click(object sender, RoutedEventArgs e)
        {
            methodPop = new PopupInterface(Color.FromArgb(255, 153, 207, 126), etrMethodName, 8, -90, "METHOD");
            methodGrid.Children.Add(methodPop);
            Grid.SetColumn(methodPop, 2);
            if (draggedItem != null && draggedItem.type == "method")
            {
                //This opens a confirmation delete popup
                methodPalette.ItemsSource = methodPop.DeletePopup(methodList, methodList[methodList.IndexOf(draggedItem)]); //remove from methodList

            }
            else
            {
                methodGrid.Children.Remove(methodPop);
            }
        }

        //deletes the tab and releases all the names from the reserved list
        public void deleteMethodAssets(int methodListIndex)
        {
            //Find the right tab that we are deleting
            foreach (TabItem tab in editorTabControl.Items)
            {
                if (tab.Name.Equals(methodList[methodListIndex].metadataList[1] + "Tab"))
                {
                    TabPage page = tab.Content as TabPage;
                    page.deleteAllParameters(); //remove all parameters names for names reserved list
                    foreach (EditorDragDropTarget EDDT in editorLists)
                    {
                        if (EDDT.Name.Equals(page.tempDragDrop.Name))
                        {
                            editorLists.RemoveAt(editorLists.IndexOf(EDDT)); //remove EDDT from editorLists
                            break;
                        }
                    }
                    string tabName = page.tempDragDrop.Name;
                    tabName = tabName.Remove(tabName.Length - 8);   //change TabPage name to method name, remove 'DragDrop'
                    tabName = tabName + "Tab";                      //adding 'Tab' to the name for comparison
                    editorTabControl.Items.RemoveAt(editorTabControl.Items.IndexOf(tab)); //remove tab

                    foreach (TabItem ti in tabList)
                    {
                        if (ti.Name.Equals(tabName))
                        {
                            tabList.RemoveAt(tabList.IndexOf(ti));
                            break;
                        }

                    }
                    break;
                }
            }
        }

        //deletes socket and updates all methods
        public void deleteParameterAsset()
        {
            foreach (TabItem tab in editorTabControl.Items)
            {
                if (draggedItem != null)
                {
                    if (tab.Name.Equals(methodList[methodList.IndexOf(draggedItem)].metadataList[1] + "Tab"))
                    {
                        TabPage page = tab.Content as TabPage;
                        page.deleteParamMethodUpdate();
                    }
                }
            }
        }

        #endregion

        #region Create Blocks
        /// <summary>
        /// Creates an instance of the program structure block by ID if it finds it in the list. 
        /// </summary>
        public static Block createProgramStructureBlock(int ID)
        {
            foreach (List<Block> package in allBlockList)
            {
                foreach (Block b in package)
                {
                    if (b.typeID == ID)
                    {
                        //Return a clone of this block
                        return b.cloneSelf(true);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Creates an instance of the reserved block by name if it finds it in the reserved list. Use this for Variables, Endifs, etc.
        /// </summary>
        public static Block createReservedBlock(String name)
        {
            foreach (Block b in reservedBlocks)
            {
                if (b.Text.Equals(name))
                {
                    //Return a clone of this block
                    return b.cloneSelf(true);
                }
            }
            return null;
        }

        /// <summary>
        /// Creates an instance of the reserved block by ID if it finds it in the reserved list. Use this for Variables, Endifs, etc.
        /// </summary>
        public static Block createReservedBlock(int ID)
        {
            foreach (Block b in reservedBlocks)
            {
                if (b.typeID == ID)
                {
                    //Return a clone of this block
                    return b.cloneSelf(true);
                }
            }
            return null;
        }

        #endregion

        #region Button Click Handlers

        //Save button click handler
        //Saves data to isolated storage
        private void saveProgram_Click(object sender, RoutedEventArgs e)
        {
            if (!freeMode)
            {
                savePop = new PopupInterface(Color.FromArgb(255, 173, 216, 230), etrVarName, -90, 90, "SAVE");
                savePop.SavePopup();
                SaveGrid.Children.Add(savePop);
                Grid.SetColumn(savePop, 2);
            }
        }

        public void saveCode(String text)
        {
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            iso.DeleteFile("save.xml");
            SaveXML.SaveToXML();

            using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("save.xml", FileMode.Open, iso))
            {
                XDocument apiDoc = XDocument.Load(isoStream);
                String savedCode = apiDoc.ToString();

                String ServiceUri = "http://genost.org/api/saveCode/" + username + "/" + text;
                //Post code to server
                WebClient cnt = new WebClient();
                cnt.UploadStringCompleted += new UploadStringCompletedEventHandler(saveCodeCompleted);
                cnt.Headers["Content-type"] = "application/json";
                cnt.Encoding = Encoding.UTF8;

                myStoryboard.Begin();
                cnt.UploadStringAsync(new Uri(ServiceUri), "POST", savedCode);
            }
        }

        //Load button click handler
        //Loads data from isolated storage
        private void loadProgram_Click(object sender, RoutedEventArgs e)
        {
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            //Only load if teh file actually exists
            if (iso.FileExists("save.xml"))
            {
                string message = "Loading in code will delete current work\n\nWould you like to load?"; //message (code) displayed
                string caption = "Load In Code?"; //header
                MessageBoxButton buttons = MessageBoxButton.OKCancel; //only allow OK and X buttons
                MessageBoxResult result = MessageBox.Show(message, caption, buttons); //display message window
                if (result == MessageBoxResult.OK)
                {
                    LoadXML.LoadFromXML();
                }
            }
        }

        //Run Simulator click handler
        private void runSim_Click(object sender, RoutedEventArgs e)
        {
            //Count up the number of unfilled sockets
            int openSockets = SocketReader.checkOpenSocks(editorDragDrop.getTreeList());
            //cannot continue if a socket is open
            if (openSockets > 0)
            {
                MessageBoxButton button = MessageBoxButton.OK; //only allow OK and X buttons
                MessageBoxResult result = MessageBox.Show("Cannot Execute: There are unfilled sockets", "Error", button); //display message window
                return;
            }

            //Write code to file in Isolated Storage
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            if (iso.FileExists("test.txt"))
            {
                iso.DeleteFile("test.txt");
            }

            if (!freeMode)
            {
                CodeParser.writeToFile(username + "%" + password + "%");
            }
            else
            {
                CodeParser.writeToFile("%%");
            }
            CodeParser.writeToFile(mazeID + "%{");
            CodeParser.parseVariable(variableList, editorDragDrop);
            CodeParser.parseCode(editorDragDrop);
            CodeParser.writeToFile("\n");
            CodeParser.parseMethods(methodList, tabList);
            CodeParser.writeToFile("}");

            //Post an attempt to the management website's data logger
            if (username != null && username != "freeModeUser" && password != null)
            {
                Uri serviceUri = new Uri("http://genost.org/api/postAttempt/" + username + "/" + password);
                WebClient downloader = new WebClient();
                downloader.OpenReadCompleted += new OpenReadCompletedEventHandler(postAttemptCompleted);
                downloader.OpenReadAsync(serviceUri);
            }

            //Generate GUID
            Guid g;
            g = Guid.NewGuid();

            string ServiceUri = "http://genost.org/api/postCode/" + g.ToString();

            String code = "";

            //Read code from isolated storage
            if (iso.FileExists("test.txt"))
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("test.txt", FileMode.Open, iso))
                {
                    using (StreamReader reader = new StreamReader(isoStream))
                    {
                        code = reader.ReadToEnd(); //read all contents of file
                    }
                }
            }

            //Post code to server
            WebClient cnt = new WebClient();
            cnt.UploadStringCompleted += new UploadStringCompletedEventHandler(uploadCodeStringCompleted);
            cnt.Headers["Content-type"] = "application/json";
            cnt.Encoding = Encoding.UTF8;
            cnt.UploadStringAsync(new Uri(ServiceUri), "POST", code);

            //Open Simulator
            HtmlPopupWindowOptions options = new HtmlPopupWindowOptions();
            options.Width = 1000;
            options.Height = 1000;
            HtmlPage.PopupWindow(new Uri("http://genost.org/genost_simulator/index.html?codeId=" + g.ToString()), "_blank", options);
            
            //Change set code indication color to orange (run sim)
            setStatusEllipse("orange");
            //messageWindow(); 
        }

        void postAttemptCompleted(object sender, OpenReadCompletedEventArgs e)
        {
        }

        void uploadCodeStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
        }

        void saveCodeCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            String result = e.Result;
            myStoryboard.Stop();
            if (result == "true")
            {
                MessageBox.Show("Save successful!");
            }
            else
            {
                MessageBox.Show("A saved file of that name already exists!");
            }
        }

        void uploadStatusStringComplete(object sender, OpenReadCompletedEventArgs e)
        {
        }

        static void pingComplete(object sender, OpenReadCompletedEventArgs e)
        {
        }

        /**
         * No idea what this is, think it's obsolete
         */
        private void RequestReady(IAsyncResult asyncResult)
        {
            HttpWebRequest request = (HttpWebRequest) asyncResult.AsyncState;
            System.IO.Stream stream = request.EndGetRequestStream(asyncResult);
            String code = "";

            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication(); //open isolated file
            if (iso.FileExists("test.txt"))
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("test.txt", FileMode.Open, iso))
                {
                    using (StreamReader reader = new StreamReader(isoStream))
                    {
                        code = reader.ReadToEnd(); //read all contents of file
                    }
                }
            }

            this.Dispatcher.BeginInvoke(delegate()
            {
                StreamWriter writer = new StreamWriter(stream);

                writer.WriteLine(code);
                writer.Flush();
                writer.Close();

                request.BeginGetResponse(new AsyncCallback(ResponseReady), request);
            });
        }

        /**
         * No idea what this is, think it's obsolete
         */
        private void ResponseReady(IAsyncResult asyncResult)
        {
            HttpWebRequest request = asyncResult.AsyncState as HttpWebRequest;

            try
            {
                HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(asyncResult);

                this.Dispatcher.BeginInvoke(delegate()
                {
                    System.IO.Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);
                    // get the result text   
                    string result = reader.ReadToEnd();
                });
            }
            catch (WebException webException)
            {
                MessageBox.Show(webException.Response.ToString());
            }
        } 

        //Click handler for Execute button (sends EXECUTE or STOP signal to robot)
        private void exCode_Click(object sender, RoutedEventArgs e)
        {
            //If the robot isn't currently running, we want to send the EXECUTE signal
            if (!robotRunning)
            {
                robotRunning = true;
                btnLabel.Text = "Stop Robot";
                string ServiceUri = "http://genost.org/api/postStatus/run";

                Uri serviceUri = new Uri(ServiceUri);
                WebClient downloader = new WebClient();
                downloader.OpenReadCompleted += new OpenReadCompletedEventHandler(uploadStatusStringComplete);
                downloader.OpenReadAsync(serviceUri);
            }
            //If the robot is currently running, we want to send the STOP signal
            else
            {
                robotRunning = false;
                btnLabel.Text = "Execute on Robot";
                string ServiceUri = "http://genost.org/api/postStatus/stop";

                Uri serviceUri = new Uri(ServiceUri);
                WebClient downloader = new WebClient();
                downloader.OpenReadCompleted += new OpenReadCompletedEventHandler(uploadStatusStringComplete);
                downloader.OpenReadAsync(serviceUri);
            }
        }

        //Click handler for Send Code To Robot button
        private void sendCode_Click(object sender, RoutedEventArgs e)
        {
            //Make sure there are no open sockets
            int openSockets = SocketReader.checkOpenSocks(editorDragDrop.getTreeList());
            //cannot continue if a socket is open
            if (openSockets > 0)
            {
                MessageBoxButton button = MessageBoxButton.OK; //only allow OK and X buttons
                MessageBoxResult result = MessageBox.Show("Cannot Execute: There are unfilled sockets", "Error", button); //display message window
                return;
            }

            //Write code to Isolated Storage
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            if (iso.FileExists("test.txt"))
            {
                iso.DeleteFile("test.txt");
            }
            CodeParser.writeToFile(mazeID + "%");
            CodeParser.writeToFile("{");
            CodeParser.parseVariable(variableList, editorDragDrop);
            CodeParser.parseCode(editorDragDrop);
            CodeParser.writeToFile("\n");
            CodeParser.parseMethods(methodList, tabList);
            CodeParser.writeToFile("}");

            messageWindow(); //For whatever reason, this function sends the code to the robot
        }

        /**
         * I don't think this does anything
         */
        /*private void roboFunctClick(object sender, RoutedEventArgs e)
        {

            if (!loadLibrary)
            {
                loadLibrary = true;
                lessonPick = new PopupInterface(Color.FromArgb(255,185,172,252), 0, 0, currentLessonId);
                roboGrid.Children.Add(lessonPick);
                Grid.SetColumn(lessonPick, 2);
                lessonPick.OkAddBtn.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(LoadOk_Click), true);
                
                
            }
        }

        private void LoadOk_Click(object sender, RoutedEventArgs e)
        {
            lessonPick.MenuPopup.IsOpen = false;
            Debug.WriteLine(lessonPick.PopupComboBox.SelectedIndex);
            currentLessonId = lessonDic[lessonPick.PopupComboBox.SelectedIndex];
            loadToolbox(lessonDic[lessonPick.PopupComboBox.SelectedIndex]);
        }*/

        //Loads the toolbox using the attached ID
        private void loadToolbox(string toolbox_id)
        {
            //Start the thread which will update the toolbox blocks
            Thread t = new Thread(new ThreadStart(threadRunning));

            //Start the Loading Ellipse animation
            myStoryboard.Begin();
            t.Start();

            //Load the toolbox
            Uri serviceUri = new Uri("http://genost.org/api/getToolbox/" + toolbox_id);
            WebClient downloader = new WebClient();
            downloader.OpenReadCompleted += new OpenReadCompletedEventHandler(getToolboxDataComplete);
            downloader.OpenReadAsync(serviceUri);
        }

        //Completion function for loading the toolbox
        void getToolboxDataComplete(object sender, OpenReadCompletedEventArgs e)
        {
            //If there were no errors
            if(e.Error == null)
            {
                //Read the toolbox data to the end
                Stream responseStream = e.Result;
                StreamReader reader = new StreamReader(responseStream);
                string text = reader.ReadToEnd();

                //Set the local toolbox doc equal to the new toolbox XML
                XDocument toolboxDoc = XDocument.Parse(text);
                xmlDoc = toolboxDoc.Descendants();
                doneLoading = true;

                //Update the Current Lesson link information
                currentLessonHL.Content = currentLessonId;
                currentLessonHL.NavigateUri = new Uri(currentLessonImage);
            }
        }

        //Click handler for the Clear Editor button
        private void clearEditor_Click(object sender, RoutedEventArgs e)
        {
            string message = "Do you want to clear current edtior?"; //message (code) displayed
            string caption = "Clear Editor?"; //header
            MessageBoxButton buttons = MessageBoxButton.OKCancel; //only allow OK and X buttons
            MessageBoxResult result = MessageBox.Show(message, caption, buttons); //display message window
            if (result == MessageBoxResult.OK)
            {
                clearCurrentTab();
            }
            
        }

        //create message window and print code output
        //---------------------------------------------------------------
        //currently not displaying a message only sending code to service
        //---------------------------------------------------------------
        private void messageWindow()
        {
            string message = ""; //message (code) displayed
            //string caption = "Code Output"; //header

            //open isolated file and read it into a string
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            if (iso.FileExists("test.txt"))
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("test.txt", FileMode.Open, iso))
                {
                    using (StreamReader reader = new StreamReader(isoStream))
                    {
                        reader.ReadLine();//skips first line of codefile, the maze ID code
                        message = reader.ReadToEnd(); //read all contents of file
                    }
                }
            }

            //Clipboard.SetText(message);
            //MessageBoxButton buttons = MessageBoxButton.OK; //only allow OK and X buttons
            //MessageBoxResult result = MessageBox.Show(message + "\nThis code has been set", caption, buttons); //display message window

            //Change set code indication color to yellow (send code)
            setStatusEllipse("yellow");

            //Send the code to the robot
            string ServiceUri = "http://genost.org/api/postCode/robotCode";

            WebClient cnt = new WebClient();
            cnt.UploadStringCompleted += new UploadStringCompletedEventHandler(uploadCodeStringCompleted);
            cnt.Headers["Content-type"] = "application/json";
            cnt.Encoding = Encoding.UTF8;
            cnt.UploadStringAsync(new Uri(ServiceUri), "POST", message);
        }

        //Old, unneeded
        /*
        private void setCodeComplete(object sender, AsyncCompletedEventArgs e)
        {
            //Change set code indication color to yellow (send code)
            setStatusEllipse("green");
        }*/

        //Change the code status indicator ellipse
        public void setStatusEllipse(string colorStatus)
        {
            switch (colorStatus)
            {
                case "orange":
                    codeStatusEllipse.Fill = new SolidColorBrush(Colors.Orange);
                    break;
                case "yellow":
                    codeStatusEllipse.Fill = new SolidColorBrush(Colors.Yellow);
                    break;
                case "green":
                    codeStatusEllipse.Fill = new SolidColorBrush(Colors.Green);
                    break;
                default:
                    codeStatusEllipse.Fill = new SolidColorBrush(Colors.Black);
                    break;
            }
        }
        
        //Clear the editor from the current tab
        private void clearCurrentTab()
        {
            //Make sure there's actually an editor to clear (There always should be)
            if (editorLists != null)
            {
                //Another check
                if (editorLists[LastSelectedTab] != null)
                {
                    //Clear it
                    editorLists[LastSelectedTab].clearTree(editorLists[LastSelectedTab].Content as ListBox);
                }
            }
        }

        #endregion

        #region Lesson load

        //Waits for the toolbox to load, stops the throbber once finished
        //Parses the toolbox once done
        private void threadRunning()
        {
            //A spinlock. Good lord, who wrote this.
            while (doneLoading == false) ;
            doneLoading = false;
            loadLibrary = false;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                //Stop the throbber
                myStoryboard.Stop();
                programStructureList.Clear(); //clear program list
                robotFunctionsList.Clear(); //clear robot list

                //Parse the toolbox
                readBlockAPI(true, xmlDoc);
                xmlDoc = null;
            });
        }

        //Waits for the toolbox to load, stops the throbber once finished
        //Also adds lessons to the lesson dictionary
        private void lessonThread()
        {
            //Disgusting spinlock
            while (doneLoading == false) ;
            doneLoading = false;
            loadLibrary = false;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                //Stop the throbber
                myStoryboard.Stop();
                if (lessons_master != null)
                {
                    //Add all lessons to the lesson dictionary
                    for (int i = 0; i < lessons_master.Count; i++)
                    {
                        lessonDic.Add(i, lessons_master[i]);
                        Debug.WriteLine(lessons_master[i]);
                    }
                }
            });
        }

        //Load in the names of the lessons
        private void loadLessons()
        {
            Uri serviceUri = new Uri("http://genost.org/api/listLessons");
            WebClient downloader = new WebClient();
            downloader.OpenReadCompleted += new OpenReadCompletedEventHandler(lessonDownloadComplete);
            downloader.OpenReadAsync(serviceUri);
        }

        //Once we've loaded the lessons, read them in.
        void lessonDownloadComplete(object sender, OpenReadCompletedEventArgs e)
        {
            if(e.Error == null)
            {
                Stream responseStream = e.Result;
                StreamReader reader = new StreamReader(responseStream);
                string text = reader.ReadToEnd();
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(String[]));
                String[] lessons = (String[])serializer.ReadObject(responseStream);
                lessons_master = new ObservableCollection<String>(lessons);

                //Load the toolbox for this particular lesson
                loadToolbox(currentLessonId);
            }
        }
        #endregion

        #region Login Prompt

        //event handler for when the page is fully loaded
        //Close the login box
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Login log = new Login();
            log.Closed += new EventHandler(loginClosed);
            log.Show();
        }

        //event handler for when the prompt is closed
        //Authenticate the user
        void loginClosed(object sender, EventArgs e)
        {
            lw = (Login)sender;

            freeMode = lw.getFreeMode();

            Uri serviceUri = new Uri("http://genost.org/api/authenticate/" + lw.getUserName() + "/" + lw.getPassword());
            WebClient downloader = new WebClient();
            downloader.OpenReadCompleted += new OpenReadCompletedEventHandler(authenticationComplete);
            downloader.OpenReadAsync(serviceUri);
        }

        //Authentication callback
        void authenticationComplete(object sender, OpenReadCompletedEventArgs e)
        {
            string result = "";

            //If we had no error, get the result
            if (e.Error == null)
            {
                Stream responseStream = e.Result;
                StreamReader reader = new StreamReader(responseStream);
                result = reader.ReadToEnd();
            }

            //The authentication was successful
            if (result == "true")
            {
                username = lw.getUserName();
                password = lw.getPassword();

                if (!freeMode)
                {
                    //Successful authentication.
                    //Store credentials in cookie (for user convenience)
                    string oldCookie = HtmlPage.Document.GetProperty("cookie") as String;
                    DateTime expiration = DateTime.UtcNow + TimeSpan.FromDays(1);
                    string cookie = String.Format("username={0};expires={1}", username, expiration.ToString("R"));
                    HtmlPage.Document.SetProperty("cookie", cookie);
                }

                //Load in lessons and current lesson too.
                Thread t = new Thread(new ThreadStart(lessonThread));
                //Start the throbber
                myStoryboard.Begin();
                t.Start();

                //Get the current lesson information
                //Note that the "currentLessonImage" service actually sends a lot of information
                Uri serviceUri = new Uri("http://genost.org/api/currentLessonImage/" + username + "/" + password);
                WebClient downloader = new WebClient();
                downloader.OpenReadCompleted += new OpenReadCompletedEventHandler(currentLessonDownloadComplete);
                downloader.OpenReadAsync(serviceUri);
            }
            //Authentication failed
            else
            {
                MessageBox.Show("Authentication Failed, please try again", "Authentication Failed", MessageBoxButton.OK);
                /*ChildWindow cw = new ChildWindow();
                cw.Content = "Authentication failed. Please try again.";
                cw.Closed += new EventHandler(tryAgainClose);
                cw.Show();*/
                tryAgainClose(null, null);
            }
        }

        //Reopens the login prompt
        void tryAgainClose(object sender, EventArgs e)
        {
            Login log = new Login();
            log.Closed += new EventHandler(loginClosed);
            log.Show();
        }

        //Called when we download the current lesson data
        void currentLessonDownloadComplete(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Stream responseStream = e.Result;
                StreamReader reader = new StreamReader(responseStream);
                string currentLesson = reader.ReadToEnd();

                //Split the received data up as needed
                string currentLessonData = currentLesson.Trim(new Char[] { '\"' });
                string[] currentLessonTokens = currentLessonData.Split('%');
                currentLessonId = currentLessonTokens[0];
                currentLessonImage = Uri.UnescapeDataString(currentLessonTokens[1]);

                //If the user has no current lesson (i.e. no curriculum)
                if (currentLessonId == "false")
                {
                    doneLoading = true;
                    MessageBox.Show("Login successful, but user " + username + " has no curriculum!\nPlease log in with a user that is assigned to a curriculum, or use Free Mode", "Missing Curriculum", MessageBoxButton.OK);
                    tryAgainClose(null, null);
                }
                //Else, if we are not in Free Mode, we show the Authentication message
                else
                {
                    if (!freeMode)
                    {
                        MessageBox.Show("Authentication successful! We will now load your current lesson.", "Authentication successful!", MessageBoxButton.OK);
                    }

                    //Load all the lessons, including the current one
                    loadLessons();
                }
            }
        }
        #endregion

        //Click handler for previous lesson button
        private void prevLessonClick(object sender, RoutedEventArgs e)
        {
            //Don't allow this to be called if we're waiting on a callback
            if(!waitingOnLessonLoad)
            {
                waitingOnLessonLoad = true;

                //Get the previous lesson information
                String uri = "http://genost.org/api/prevLessonImage/" + username + "/" + password;
                if (freeMode)
                {
                    uri += "/" + currentLessonId;
                }
                Uri serviceUri = new Uri(uri);
                WebClient downloader = new WebClient();
                downloader.OpenReadCompleted += new OpenReadCompletedEventHandler(changeLessonDownloadComplete);
                downloader.OpenReadAsync(serviceUri);
            }
        }

        //Click handler for next lesson button
        private void nextLessonClick(object sender, RoutedEventArgs e)
        {
            //Don't allow this to be called if we're waiting on a callback
            if(!waitingOnLessonLoad)
            {
                waitingOnLessonLoad = true;
                String uri = "http://genost.org/api/nextLessonImage/" + username + "/" + password;
                if (freeMode)
                {
                    uri += "/" + currentLessonId;
                }
                Uri serviceUri = new Uri(uri);
                WebClient downloader = new WebClient();
                downloader.OpenReadCompleted += new OpenReadCompletedEventHandler(changeLessonDownloadComplete);
                downloader.OpenReadAsync(serviceUri);
            }
        }

        //Callback for when we've finished downloading a lesson by clicking either NEXT or PREVIOUS
        void changeLessonDownloadComplete(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Stream responseStream = e.Result;
                StreamReader reader = new StreamReader(responseStream);
                string changeLesson = reader.ReadToEnd();

                //Split the downloaded data up as needed
                changeLesson = changeLesson.Trim(new Char[] { '\"' });
                string[] changeLessonTokens = changeLesson.Split('%');
                string newId = changeLessonTokens[0];

                //Handle possible edge cases
                if (newId == "end")
                {
                    MessageBox.Show("Your current lesson is the last one in your curriculum.", "Last Lesson", MessageBoxButton.OK);
                }
                else if (newId == "begin")
                {
                    MessageBox.Show("Your current lesson is the first one in your curriculum.", "First Lesson", MessageBoxButton.OK);
                }
                else if (newId == "false")
                {
                    MessageBox.Show("We're sorry, an error has occurred.", "Error", MessageBoxButton.OK);
                }
                else
                {
                    currentLessonId = newId;
                    currentLessonImage = Uri.UnescapeDataString(changeLessonTokens[1]);
                    loadToolbox(currentLessonId);
                }

                waitingOnLessonLoad = false;
            }
        }

        //We want to ping every time some activity takes place
        //This function handles that.
        public static void ping()
        {
            if (username != null && username != "freeModeUser" && password != null)
            {
                Uri serviceUri = new Uri("http://genost.org/api/postActivity/" + username + "/" + password);
                WebClient downloader = new WebClient();
                downloader.OpenReadCompleted += new OpenReadCompletedEventHandler(pingComplete);
                downloader.OpenReadAsync(serviceUri);
            }
        }
    }
}
