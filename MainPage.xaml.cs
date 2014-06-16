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
        public ObservableCollection<String> lessons = null; //lesson array
        private robotService.ServiceClient client; //client to send info to robot
        private serverXML.XmlWebServiceClient xmlClient;
        private lessonService.XmlWebServiceClient lessonClient;
        

        private Dictionary<int, string> lessonDic;
        private string mazeID;
        
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
        private MainPage()
        {
            
            InitializeComponent();
            
            //Service
            client = new robotService.ServiceClient();
            xmlClient = new serverXML.XmlWebServiceClient();
            lessonClient = new lessonService.XmlWebServiceClient();
            

            lessonDic = new Dictionary<int, string>();
            loadLessons();
            
            //Lists
            programStructureList = new ObservableCollection<Block>();
            robotFunctionsList = new ObservableCollection<Block>();
            variableList = new ObservableCollection<Block>();
            methodList = new ObservableCollection<Block>();

            //method lists
            tabList = new List<TabItem>();

            communicate = new DragDropTargetCommunication();

            nameList = new List<String>();
            editorLists = new List<EditorDragDropTarget>();
            editorLists.Add(editorDragDrop);

            readBlockAPI(false, xmlDoc);
            
            //Set ItemsSource of ListBox to desired Lists
            blockPalette.ItemsSource = programStructureList;
            robotPalette.ItemsSource = robotFunctionsList;
            variablePalette.ItemsSource = variableList;
            methodPalette.ItemsSource = methodList;
            
            //Allow blocks to be placed in trash
            editorPalette.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Handle_EditorMouseDown), true);
            editorPalette.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(Handle_EditorMouseUp), true);

            //Add blocks from package
            blockPalette.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Handle_ProgramMouseDown), true);
            robotPalette.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Handle_RobotMouseDown), true);

            //Variable panel
            variablePalette.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Handle_VarMethMouseDown), true);

            //Method panel
            methodPalette.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Handle_VarMethMouseDown), true);

            //Waiting until the page is loaded before launching login method call
            this.Loaded += MainPage_Loaded;
        }

        

        private bool readBlockAPI(bool loadFromServer, IEnumerable<XElement> xmlFile)
        {
            BlockAPIReader r = null;
            if (loadFromServer)
            {
                //do things
                r = new BlockAPIReader(xmlFile);
            }
            else
            {
                r = new BlockAPIReader();
            }
            allBlockList = r.readBlockDefinitions();

            packageNameList = r.getPackageNames();
            reservedBlocks = r.getReservedBlocks();

            mazeID = r.getMazeID();

            //Add package marker blocks to the program structures palette
            for (int i = 0; i < allBlockList.Count; i++)
            {
                
                //First, check to make sure the package contains at least one non-program reserved block
                bool containsNonReserved = false;
                //Second, check to see which panel the package needs to be added to
                bool isPartOfProgramPanel = true;
                foreach (Block b in allBlockList[i])
                {
                    if (!b.flag_programOnly)
                    {
                        containsNonReserved = true;
                        if (b.flag_robotOnly)
                        {
                            isPartOfProgramPanel = false;
                        }
                        break;
                    }
                }
                if (containsNonReserved && isPartOfProgramPanel)
                {
                    //If it does, create a marker block for that package
                    Block packageBlock = new Block(packageNameList[i], ((Block)allBlockList[i][0]).blockColor);
                    packageBlock.LayoutRoot.Width = blockPalette.Width-30;
                    packageBlock.flag_isPackage = true;
                    programStructureList.Add(packageBlock);
                }
                else if (containsNonReserved && !isPartOfProgramPanel)
                {
                    Block packageBlock = new Block(packageNameList[i], ((Block)allBlockList[i][0]).blockColor);
                    packageBlock.LayoutRoot.Width = blockPalette.Width - 30;
                    packageBlock.flag_isPackage = true;
                    robotFunctionsList.Add(packageBlock);
                }
            }

            if (allBlockList != null)
                return true;
            else
                return false;
        }
        #endregion

        //Here there be draggin' (handlers)
        #region Handlers
        private void Handle_VarMethMouseDown(object sender, MouseButtonEventArgs e)
        {
            trashDragDrop.AllowAdd = false; // can move these blocks to trash
            ListBox listBox = sender as ListBox;
            draggedItem = (Block)listBox.SelectedItem;
        }

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

        private void Handle_EditorMouseUp(object sender, MouseEventArgs args)
        {
            for (int i = 0; i < this.editorPalette.Items.Count; i++)
            {
                ((Block)this.editorPalette.Items[i]).index = (i);
            }
        }

        private void Handle_ProgramMouseDown(object sender, MouseEventArgs args)
        {
            trashDragDrop.AllowAdd = false; // can move these blocks to trash
            ListBox listbox = sender as ListBox;
            int index = listbox.SelectedIndex;
            if (listbox.SelectedItem != null && listbox.SelectedItem.GetType().Equals(typeof(Block)))
            {
                Block temp = listbox.SelectedItem as Block;

                if (temp.flag_isPackage && (index == listbox.Items.Count - 1 || ((Block)listbox.Items[index + 1]).flag_isPackage))
                {
                    foreach (String s in packageNameList)
                    {
                        if (s.Equals(temp.Text))
                        {
                            List<Block> list = allBlockList[packageNameList.IndexOf(s)];
                            listbox.ItemsSource = null;
                            for (int i = list.Count - 1; i >= 0; i--)
                            {
                                Block tempBlock = list[i] as Block;
                                tempBlock.Margin = new Thickness(50, 0, 0, 0);
                                string tempName = tempBlock.returnName();
                                if (tempName != null)
                                    tempBlock.fore.Text = tempName;
                                tempBlock.LayoutRoot.Width = blockPalette.Width - 80;
                                programStructureList.Insert(index + 1, list[i] as Block);
                            }
                            listbox.ItemsSource = programStructureList;
                        }
                    }
                }
                else if (temp.flag_isPackage && (index != listbox.Items.Count - 1 || !((Block)listbox.Items[index + 1]).flag_isPackage))
                {
                    int startPoint = index;
                    int endPoint =startPoint + 1;
                    while (endPoint < listbox.Items.Count - 1 && !((Block)listbox.Items[endPoint]).flag_isPackage)
                    {
                        endPoint++;
                    }

                    if (endPoint != listbox.Items.Count - 1)
                        endPoint--;
                    else if (((Block)listbox.Items[endPoint]).flag_isPackage)
                        endPoint--;

                    listbox.ItemsSource = null;
                    for (int i = endPoint; i > startPoint; i--)
                    {
                        programStructureList.RemoveAt(i);
                    }
                    listbox.ItemsSource = programStructureList;
                }
            }
        }

        private void Handle_RobotMouseDown(object sender, MouseEventArgs args)
        {
            trashDragDrop.AllowAdd = false; // can move these blocks to trash
            ListBox listbox = sender as ListBox;
            int index = listbox.SelectedIndex;
            if (listbox.SelectedItem != null && listbox.SelectedItem.GetType().Equals(typeof(Block)))
            {
                Block temp = listbox.SelectedItem as Block;

                if (temp.flag_isPackage && (index == listbox.Items.Count - 1 || ((Block)listbox.Items[index + 1]).flag_isPackage))
                {
                    foreach (String s in packageNameList)
                    {
                        if (s.Equals(temp.Text))
                        {
                            List<Block> list = allBlockList[packageNameList.IndexOf(s)];
                            listbox.ItemsSource = null;
                            for (int i = list.Count - 1; i >= 0; i--)
                            {
                                Block tempBlock = list[i] as Block;
                                tempBlock.Margin = new Thickness(50, 0, 0, 0);
                                string tempName = tempBlock.returnName();
                                if (tempName != null)
                                    tempBlock.fore.Text = tempName;
                                tempBlock.LayoutRoot.Width = blockPalette.Width - 80;
                                robotFunctionsList.Insert(index + 1, list[i] as Block);
                            }
                            listbox.ItemsSource = robotFunctionsList;
                        }
                    }
                }
                else if (temp.flag_isPackage && (index != listbox.Items.Count - 1 || !((Block)listbox.Items[index + 1]).flag_isPackage))
                {
                    int startPoint = index;
                    int endPoint = startPoint + 1;
                    while (endPoint < listbox.Items.Count - 1 && !((Block)listbox.Items[endPoint]).flag_isPackage)
                    {
                        endPoint++;
                    }

                    if (endPoint != listbox.Items.Count - 1)
                        endPoint--;
                    else if (((Block)listbox.Items[endPoint]).flag_isPackage)
                        endPoint--;

                    listbox.ItemsSource = null;
                    for (int i = endPoint; i > startPoint; i--)
                    {
                        robotFunctionsList.RemoveAt(i);
                    }
                    listbox.ItemsSource = robotFunctionsList;
                }
            }
        }

        /*
         * Selection Change Event
         * Save the new slected index to LastSelectedTab
         */
        private void Handle_TabSelectedChange(object sender, SelectionChangedEventArgs args)
        {
            TabControl tab = sender as TabControl;
            if (this.LastSelectedTab != tab.SelectedIndex)
            {
                this.LastSelectedTab = tab.SelectedIndex;
                if (editorLists != null)
                {
                    string tabName = editorLists[LastSelectedTab].Name;
                    tabName = tabName.Remove(tabName.Length - 8);   //change TabPage name to method name, remove 'DragDrop'
                    if (tabName.Equals("editor"))
                        tabName = "Main";
                    clearBtn.Content = "CLEAR " + tabName;
                }
            }
        }
        
        //handles error messages
        public static void errorMessage(string message)
        {
            //errorMsg.text = message;
            //put error code here
        }
        #endregion

        #region Variable Creation
        //used to create
        private void createVariableBtn_Click(object sender, RoutedEventArgs e)
        {
            varPop = new PopupInterface(Color.FromArgb(255, 255, 174, 201), etrVarName, 8, -90, "VARIABLE");
            varPop.PopupComboBox.Items.RemoveAt(3);
            variableGrid.Children.Add(varPop);
            Grid.SetColumn(varPop, 2);
            varPop.OkAddBtn.Click += new RoutedEventHandler(variableAdd_Click);
            varPop.CreatePopup();
        }

        //used to create
        private void variableAdd_Click(object sender, RoutedEventArgs e)
        {
            string text = varPop.PopupTextBox.Text;
            bool fail = false;
            if (varPop.PopupComboBox.SelectedItem != null && (!text.Equals(etrVarName) && !text.Equals("")))
            {
                foreach (string s in nameList)
                {
                    if (s.Equals(text))
                    {
                        fail = true;
                        break;
                    }
                }
                if (!fail)
                {
                    nameList.Add(text);
                    varPop.MenuPopup.IsOpen = false;
                    createVariable(text, (varPop.PopupComboBox.SelectionBoxItem as TextBlock).Text);
                }
            }
        }

        //used to create
        public void createVariable(string name, string type)
        {
            Block newVariable = createReservedBlock("VARIABLE");
            newVariable.metadataList[0] = type;
            newVariable.returnType = newVariable.metadataList[0];
            newVariable.metadataList[1] = name;
            newVariable = newVariable.cloneSelf(true);
            newVariable.LayoutRoot.Width = blockPalette.Width - 30;
            variableList.Add(newVariable);
        }

        //used to edit
        private void deleteVariableBtn_Click(object sender, RoutedEventArgs e)
        {
            varPop = new PopupInterface(Color.FromArgb(255, 255, 174, 201), etrVarName, 8, -90, "VARIABLE");
            variableGrid.Children.Add(varPop);
            Grid.SetColumn(varPop, 2);
            if (draggedItem != null)
                variablePalette.ItemsSource = varPop.DeletePopup(variableList, variableList[variableList.IndexOf(draggedItem)]);
            else
                variableGrid.Children.Remove(varPop);
        }

        #endregion

        #region Method Creation
        //used to create
        private void createMethodBtn_Click(object sender, RoutedEventArgs e)
        {
            methodPop = new PopupInterface(Color.FromArgb(255, 153, 207, 126), etrMethodName, 8, -90, "METHOD");
            //methodPop.PopupComboBox.Items.Add((new ComboBoxItem().Content = ((new TextBlock()).Text = "VOID"))); 
            methodGrid.Children.Add(methodPop);
            Grid.SetColumn(methodPop, 2);
            methodPop.OkAddBtn.Click += new RoutedEventHandler(methodAdd_Click);
            methodPop.CreatePopup();
        }

        //used to create
        private void methodAdd_Click(object sender, RoutedEventArgs e)
        {
            string text = methodPop.PopupTextBox.Text;
            bool fail = false;
            if (methodPop.PopupComboBox.SelectedItem != null && (!text.Equals(etrMethodName) && !text.Equals("")))
            {
                foreach (string s in nameList)
                {
                    if (s.Equals(text))
                    {
                        fail = true;
                        break;
                    }
                }
                if (!fail)
                {
                    nameList.Add(text);
                    methodPop.MenuPopup.IsOpen = false;
                    createMethod(methodPop.PopupTextBox.Text, (methodPop.PopupComboBox.SelectionBoxItem as TextBlock).Text);
                }
            }
        }

        //used to create
        public void createMethod(string name, string type)
        {
            Block newMethod = createReservedBlock("METHOD");
            newMethod.metadataList[1] = name;
            newMethod.returnType = type;
            newMethod = newMethod.cloneSelf(false);
            newMethod.LayoutRoot.Width = blockPalette.Width - 30;
            methodList.Add(newMethod);
            createMethodTab(name, type);
        }

        //create the new method tab and add to the editorTabControl and add new tab EDDT to editorList
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
                    tabPage.returnType.SelectedIndex = tabPage.returnType.Items.IndexOf(comboBox);  //set the index
            }
            
            tabPage.CreateMethodReturnBlock();
        }

        //used to edit
        private void deleteMethodBtn_Click(object sender, RoutedEventArgs e)
        {
            methodPop = new PopupInterface(Color.FromArgb(255, 153, 207, 126), etrMethodName, 8, -90, "METHOD");
            methodGrid.Children.Add(methodPop);
            Grid.SetColumn(methodPop, 2);
            if (draggedItem != null)
            {
                methodPalette.ItemsSource = methodPop.DeletePopup(methodList, methodList[methodList.IndexOf(draggedItem)]); //remove from methodList
                
            }
            else
                methodGrid.Children.Remove(methodPop);
        }

        //deletes the tab and releases all the names from the reserved list
        public void deleteMethodAssets(int methodListIndex)
        {
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

        private void saveProgram_Click(object sender, RoutedEventArgs e)
        {
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            if (iso.FileExists("save.xml"))
            {
                string message = "Saving code will overwright previous save data\n\nWould you like to save?"; //message (code) displayed
                string caption = "Save Code?"; //header
                MessageBoxButton buttons = MessageBoxButton.OKCancel; //only allow OK and X buttons
                MessageBoxResult result = MessageBox.Show(message, caption, buttons); //display message window
                if (result == MessageBoxResult.Cancel)
                {
                    return; 
                }
                iso.DeleteFile("save.xml");
            }
            SaveXML.SaveToXML();
        }

        private void loadProgram_Click(object sender, RoutedEventArgs e)
        {
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
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

        private void runSim_Click(object sender, RoutedEventArgs e)
        {
            int openSockets = SocketReader.checkOpenSocks(editorDragDrop.getTreeList());
            //cannot continue if a socket is open
            if (openSockets > 0)
            {
                MessageBoxButton button = MessageBoxButton.OK; //only allow OK and X buttons
                MessageBoxResult result = MessageBox.Show("Cannot Execute: There are unfilled sockets", "Error", button); //display message window
                return;
            }

            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            if (iso.FileExists("test.txt"))
            {
                iso.DeleteFile("test.txt");
            }

            CodeParser.writeToFile(mazeID + "%{");
            CodeParser.parseVariable(variableList, editorDragDrop);
            CodeParser.parseCode(editorDragDrop);
            CodeParser.writeToFile("\n");
            CodeParser.parseMethods(methodList, tabList);
            CodeParser.writeToFile("}");

            Guid g;
            g = Guid.NewGuid();

            string ServiceUri = "http://venus.eas.asu.edu/WSRepository/eRobotic2/codeRestSvc/Service.svc/setCode/" + g.ToString();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServiceUri);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.BeginGetRequestStream(new AsyncCallback(RequestReady), request);

            HtmlPopupWindowOptions options = new HtmlPopupWindowOptions();
            options.Width = 1000;
            options.Height = 1000;

            HtmlPage.PopupWindow(new Uri("http://venus.eas.asu.edu/WSRepository/eRobotic2/javaSim/MainApplet.html?codeId=" + g.ToString()), "_blank", options);
            
            //Change set code indication color to orange (run sim)
            setStatusEllipse("orange");
            //messageWindow(); 
        }

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
                    int asdf = 0;
                });
            }
            catch (WebException webException)
            {
                MessageBox.Show(webException.Response.ToString());
            }
        } 

        private void exCode_Click(object sender, RoutedEventArgs e)
        {
            if (!robotRunning)
            {
                robotRunning = true;
                btnLabel.Text = "Stop Robot";               
                client.SetStatusCompleted += executeCodeComplete;
                client.SetStatusAsync("run");
            }
            else
            {
                robotRunning = false;
                btnLabel.Text = "Execute on Robot";
                client.SetStatusCompleted += stopCodeComplete;
                client.SetStatusAsync("stop");
            }

            
        }

        private void stopCodeComplete(object sender, AsyncCompletedEventArgs e)
        {
            client.SetStatusCompleted -= stopCodeComplete;
        }

        private void executeCodeComplete(object sender, AsyncCompletedEventArgs e)
        {
            client.SetStatusCompleted -= executeCodeComplete;
        }

        private void sendCode_Click(object sender, RoutedEventArgs e)
        {
            int openSockets = SocketReader.checkOpenSocks(editorDragDrop.getTreeList());
            //cannot continue if a socket is open
            if (openSockets > 0)
            {
                MessageBoxButton button = MessageBoxButton.OK; //only allow OK and X buttons
                MessageBoxResult result = MessageBox.Show("Cannot Execute: There are unfilled sockets", "Error", button); //display message window
                return;
            }

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

            messageWindow();
        }

        private void roboFunctClick(object sender, RoutedEventArgs e)
        {

            if (!loadLibrary)
            {
                loadLibrary = true;
                //roboLoadBtn.Content = "UPDATE";
                lessonPick = new PopupInterface(Color.FromArgb(255,185,172,252), 0,0);
                roboGrid.Children.Add(lessonPick);
                Grid.SetColumn(lessonPick, 2);
                lessonPick.OkAddBtn.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(LoadOk_Click), true);
                
                
            }
            /*else
            {
                if (xmlDoc == null)
                {
                    string message = "Code is currently loading from server.\n\nPlease allow a few seconds and try again."; //message (code) displayed
                    string caption = "Loading From Server"; //header
                    MessageBoxButton buttons = MessageBoxButton.OK; //only allow OK and X buttons
                    MessageBoxResult result = MessageBox.Show(message, caption, buttons); //display message window
                }
                else
                {
                    loadLibrary = false;
                    roboLoadBtn.Content = "LOAD";
                    programStructureList.Clear(); //clear program list
                    robotFunctionsList.Clear(); //clear robot list
                    readBlockAPI(true, xmlDoc);
                    xmlDoc = null;
                }
            }*/
        }       

        private void LoadOk_Click(object sender, RoutedEventArgs e)
        {
            lessonPick.MenuPopup.IsOpen = false;
            Debug.WriteLine(lessonPick.PopupComboBox.SelectedIndex);
            Thread t = new Thread(new ThreadStart(threadRunning));
            myStoryboard.Begin();
            t.Start();
            Debug.WriteLine(lessonDic[lessonPick.PopupComboBox.SelectedIndex]);
            lessonClient.getLessonPlanCompleted += getXmlDataComplete;
            lessonClient.getLessonPlanAsync(lessonDic[lessonPick.PopupComboBox.SelectedIndex]);
        }

        private void getXmlDataComplete(object sender, lessonService.getLessonPlanCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                xmlDoc = e.Result;
            }
            lessonClient.getLessonPlanCompleted -= getXmlDataComplete;
        }


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
            
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication(); //open isolated file
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

            client.SetCodeCompleted += setCodeComplete;
            client.SetCodeAsync(message);

        }

        private void setCodeComplete(object sender, AsyncCompletedEventArgs e)
        {
            client.SetStatusCompleted -= setCodeComplete;

            //Change set code indication color to yellow (send code)
            setStatusEllipse("green");
        }

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
        
        //Clear the current Tab data
        private void clearCurrentTab()
        {
            if (editorLists != null)
            {
                if (editorLists[LastSelectedTab] != null)
                {
                    editorLists[LastSelectedTab].clearTree(editorLists[LastSelectedTab].Content as ListBox);
                }
            }
        }

        #endregion

        #region Lesson Plan load

        //loads the lesson blocks in and stops the loading throbber once finished
        private void threadRunning()
        {
            while (xmlDoc == null) ;
            loadLibrary = false;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                myStoryboard.Stop();
                programStructureList.Clear(); //clear program list
                robotFunctionsList.Clear(); //clear robot list
                readBlockAPI(true, xmlDoc);
                xmlDoc = null;
            });
        }

        //loads the lesson options in and stops the loading throbber once finished
        private void lessonThread()
        {
            while (lessons == null) ;
            loadLibrary = false;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                myStoryboard.Stop();
                for (int i = 0; i < lessons.Count; i++)
                {
                    lessonDic.Add(i, lessons[i]);
                    Debug.WriteLine(lessons[i]);
                }
            });
        }

        //loads lessons options
        private void loadLessons()
        {
            Thread t = new Thread(new ThreadStart(lessonThread));
            myStoryboard.Begin();
            t.Start();
            lessonClient.listLessonPlansCompleted += getLessonDataComplete;
            lessonClient.listLessonPlansAsync();
        }

        //service call to get the lessons from server
        private void getLessonDataComplete(object sender, lessonService.listLessonPlansCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                lessons = e.Result;
            }
            lessonClient.listLessonPlansCompleted -= getLessonDataComplete;
        }
        #endregion

        #region Login Prompt

        //event handler for when the page is fully loaded
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Login log = new Login();
            log.Closed += new EventHandler(loginClosed);
            log.Show();
        }

        //event handler for when the prompt is closed
        void loginClosed(object sender, EventArgs e)
        {
            Login lw = (Login)sender;

            if (lw.DialogResult == true)
            {
                //do stuff for successful login here
                //note that password and username are currently stored in the login class
            }
            else if (lw.DialogResult == false)
            {
                //do stuff for failed login here

                //this code just reopens the prompt in the case of cancel or X are pressed for now, forcing the user to input their info
                Login log = new Login();
                log.Closed += new EventHandler(loginClosed);
                log.Show();
            }
        }
        #endregion
    }
}
