using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CapGUI.Parsing;

namespace CapGUI
{
    public partial class TabPage : Page
    {

        #region Constants and Globals
        private PopupInterface pop;
        private ObservableCollection<Block> parameterList;
        //private ObservableCollection<Block> returnList;
        private Block selected;
        private string etrParaName = "Enter Parameter Name";
        #endregion

        public TabPage()
        {
            InitializeComponent();

            parameterList = new ObservableCollection<Block>();
            //returnList = new ObservableCollection<Block>();
            
            parameterBox.ItemsSource = parameterList;
            //returnBox.ItemsSource = returnList;
            
            parameterBox.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Handle_ParameterMouseDown), true);
            returnType.SelectionChanged += new SelectionChangedEventHandler(Handle_returnTypeComboBox);
        }

        #region Handlers
        //Change return type of block when return type is changed in tab
        private void Handle_returnTypeComboBox(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            ComboBoxItem ci = (ComboBoxItem)cb.SelectedItem;
            TextBlock tb = (TextBlock)ci.Content;   //get the TextBlock aka return type

            Block methodBlock = getMethodBlock();
            if (methodBlock != null)
            {
                methodBlock.returnType = tb.Text;
                UpdateMethodBlocks(methodBlock);
                CreateMethodReturnBlock();
            }
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("I LOVE MONKEYS");
        }

        private void Handle_ParameterMouseDown(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            selected = (Block)listBox.SelectedItem;
        }
        #endregion

        #region Adding Parameteres
        private void addParamBtn_Click(object sender, RoutedEventArgs e)
        {
            pop = new PopupInterface(Color.FromArgb(255, 153, 207, 126), etrParaName, 8, 20, "PARAMETER");
            pop.PopupComboBox.Items.RemoveAt(3);
            LayoutRoot.Children.Add(pop);
            Grid.SetColumn(pop, 0);
            Grid.SetRow(pop, 1);
            pop.OkAddBtn.Click += new RoutedEventHandler(parameterAdd_Click);
            pop.CreatePopup();
        }

        //used to create
        private void parameterAdd_Click(object sender, RoutedEventArgs e)
        {
            string text = pop.PopupTextBox.Text;
            bool fail = false;
            if (pop.PopupComboBox.SelectedItem != null && (!text.Equals(etrParaName) && !text.Equals(""))) //has type and name
            {
                foreach (string s in MainPage.nameList) //name is not unique
                {
                    if (s.Equals(text))
                    {
                        fail = true;
                        break;
                    }
                }
                if (!fail) //name is unique add parameter
                {
                    pop.MenuPopup.IsOpen = false;
                    MainPage.nameList.Add(text);
                    createParameter(pop.PopupTextBox.Text, (pop.PopupComboBox.SelectionBoxItem as TextBlock).Text);
                    
                }
            }
        }

        public void createParameter(string name, string type)
        {
            Block newParameter = MainPage.createReservedBlock("PARAMETER");
            newParameter.metadataList[0] = type;
            newParameter.metadataList[1] = name;
            newParameter = newParameter.cloneSelf(true);
            newParameter.LayoutRoot.Width = parameterBox.Width - 40;
            newParameter.LayoutRoot.Height = 20;
            newParameter.returnType = newParameter.metadataList[0];
            parameterList.Add(newParameter);
            Block methodBlock = getMethodBlock();
            if (methodBlock != null)
            {
                methodBlock.parameterList.Add(newParameter.metadataList[0]);
                UpdateMethodBlocks(methodBlock);
            }
            
        }
        #endregion

        #region Deleting Parameters
        private void deleteParamBtn_Click(object sender, RoutedEventArgs e)
        {
            pop = new PopupInterface(Color.FromArgb(255, 153, 207, 126), etrParaName, 8, 70, "PARAMETER");
            LayoutRoot.Children.Add(pop);
            Grid.SetColumn(pop, 0);
            Grid.SetRow(pop, 1);
            //need to add catch for no value selected
            if (selected != null && parameterList.IndexOf(selected) < parameterList.Count)
            {
                parameterBox.ItemsSource = pop.DeletePopup(parameterList, parameterList[parameterList.IndexOf(selected)]);  
            }
            else
                LayoutRoot.Children.Remove(pop);

        }

        //removes the socket from the methods after parameter is deleted
        public void deleteParamMethodUpdate()
        {
            Block methodBlock = getMethodBlock();
            if (methodBlock != null)
            {
                methodBlock.parameterList.RemoveAt(parameterList.IndexOf(selected));
                UpdateMethodBlocks(methodBlock);
            }
        }

        //remove all parameter names from the reserved name lists in MainPage
        public void deleteAllParameters()
        {
            for (int i = MainPage.nameList.Count - 1; i >= 0; i--)
            {
                foreach (Block b in parameterList)
                {
                    if (b.metadataList[1].Equals(MainPage.nameList[i]))
                    {
                        //Debug.WriteLine(MainPage.nameList[i]);
                        MainPage.nameList.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        #endregion

        #region Method and Parameter access
        //Finds the method associated with the tab and return that method block
        private Block getMethodBlock()
        {
            Grid g = (Grid)returnType.Parent;
            TabPage tp = (TabPage)g.Parent;
            TabItem ti = (TabItem)tp.Parent;    //get the TabPage for getting method block
            string tabName = ti.Name;
            tabName = tabName.Remove(tabName.Length - 3);   //change TabPage name to method name, remove 'Tab'

            foreach (Block b in MainPage.methodList) //find method block
            {
                if (b.metadataList[1].Equals(tabName))
                    return b;
            }
            return null;
        }

        //finds each method and updates that method in the editor windows of all tabs
        private void UpdateMethodBlocks(Block methodBlock)
        {  
            foreach (EditorDragDropTarget EDDT in MainPage.editorLists)
            {
                //replace based on block user generated name
                //((ListBox)EDDT.Content).ItemsSource = EDDT.switchBlocks(methodBlock.cloneSelf(true), true);
                EDDT.switchBlocks(EDDT.Content as ListBox, methodBlock.cloneSelf(true), true);
                ObservableCollection<Block> collection = ((ListBox)EDDT.Content).ItemsSource as ObservableCollection<Block>;
                Parsing.SocketReader.ReplaceMethodBlocks(collection, methodBlock.cloneSelf(true));

                //change was made
                MainPage.communicate.changeCodeColorStatus();
            }
        }

        //apparently the parameter blocks are inaccessible from outside this class, regardless of avenue taken, so this method returns the parameter block at the given index
        public Block getParamAtIndex(int i)
        {
            return parameterList.ElementAt(i);
        }

        //used in load for getting the correct parameter
        public Block getParamForName(string name)
        {
            foreach (Block b in parameterList)
            {
                Debug.WriteLine(b.Text + " " + b.metadataList[1] + " " + name);
                if (b.metadataList[1].Equals(name))
                {
                    return b.cloneSelf(true);
                }
            }
            return null;
        }

        //used in load for getting the return block for that method
        public Block getReturnBlock()
        {
            if (returnBox.Items.Count >= 1)
                return ((Block)returnBox.Items[0]).cloneSelf(true);
            else
                return null;
        }
        
        //creates new ObservableCollection with new return block and set returnBox item source to new OC list
        public void CreateMethodReturnBlock()
        {
            Block newReturnBlock = MainPage.createReservedBlock("RETURN");
            ComboBoxItem ci = (ComboBoxItem)returnType.SelectedItem;
            TextBlock tb = (TextBlock)ci.Content;   //get the TextBlock aka return type
            //newReturnBlock.metadataList[1] = tb.Text;
            newReturnBlock.returnType = tb.Text;
            newReturnBlock = newReturnBlock.cloneSelf(false);
            newReturnBlock.LayoutRoot.Width = returnBox.Width - 20;
            newReturnBlock.LayoutRoot.Height = returnBox.Height - 15;
            ObservableCollection<Block> returnList = new ObservableCollection<Block>();
            if(!tb.Text.Equals("VOID"))
                returnList.Add(newReturnBlock);
            returnBox.ItemsSource = returnList;
            //UpdateMethodBlocks();
            foreach (EditorDragDropTarget EDDT in MainPage.editorLists)
            {
                //replace based on block name
                ((ListBox)EDDT.Content).ItemsSource = EDDT.switchBlocks(newReturnBlock.cloneSelf(true), false);
            }
        }
        #endregion
    }
}