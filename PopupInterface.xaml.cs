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
using System.Collections.ObjectModel;
using System.Diagnostics;
using CapGUI.Parsing;
using System.IO.IsolatedStorage;
using CapGUI.xml;
using System.Xml.Linq;
using System.IO;

namespace CapGUI
{
    /**
     * This class is used to create a popup-type item, such as the variable creation window, the method creation window, etc.
     */
    public partial class PopupInterface : UserControl
    {
        private ObservableCollection<Block> list;
        private string source;
        private int references = 0;
        private Block deletionBlock;

        //Constructor with source
        //Source is used to track what kind of popup this is (i.e. variable, method, etc.)
        public PopupInterface(Color color, String initalText, int verticalOffset, int horizontalOffset, string source)
        {
            InitializeComponent();
            MenuCanvas.Background = new SolidColorBrush(color);
            PopupTextBox.Text = initalText;
            PopupTextBox.IsReadOnly = true;
            this.source = source;
            MenuPopup.IsOpen = true;
            MenuPopup.VerticalOffset = verticalOffset;
            MenuPopup.HorizontalOffset = horizontalOffset;
            
            PopupTextBox.GotFocus += new RoutedEventHandler(Child_OnHasFocus);
            MenuPopup.LostFocus += new RoutedEventHandler(Child_OnLostFocus);
        }

        //Constructor without source
        //Never actually used, appears to have been intended for some sort of lesson selection
        public PopupInterface(Color color, int verticalOffset, int horizontalOffset, string currentItem)
        {
            InitializeComponent();
            MenuCanvas.Background = new SolidColorBrush(color);
            MenuPopup.IsOpen = true;
            MenuPopup.VerticalOffset = verticalOffset;
            MenuPopup.HorizontalOffset = horizontalOffset;
            MenuCanvas.Children.Remove(PopupTextBox);
            MenuCanvas.Children.Remove(OkEditBtn);
            MenuCanvas.Children.Remove(PopupDeleteInfo);
            MenuCanvas.Children.Remove(DeleteConfirm);
            MenuCanvas.Children.Remove(type);

            TextBlock text = new TextBlock();
            text.Text = "Choose a Lesson";
            text.FontSize = 16;
            text.Width = MenuCanvas.Width;
            text.TextAlignment = TextAlignment.Center;
            
            MenuCanvas.Children.Insert(0, text);

            PopupComboBox.Items.Clear();
            Canvas.SetLeft(PopupComboBox, 25);

            if (MainPage.Instance.lessons_master != null)
            {
                for (int i = 0; i < MainPage.Instance.lessons_master.Count; i++)
                {
                    PopupComboBox.Items.Add(((new ComboBoxItem()).Content = MainPage.Instance.lessons_master[i]));
                }
            }

            int selectedIndex = PopupComboBox.Items.IndexOf(currentItem);
            PopupComboBox.SelectedIndex = selectedIndex;

            OkAddBtn.Content = "OK";

        }

        //Focus handler for textbox. Deletes the placeholder text
        private void Child_OnHasFocus(object sender, RoutedEventArgs e)
        {
            PopupTextBox.IsReadOnly = false;
            PopupTextBox.Text = "";
        }

        //Click handler for clicking "cancel" in the Variable popup
        private void variableCancel_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
        }

        //Click handler for moving focus outside of the textbox. Closes the textbox
        private void Child_OnLostFocus(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
        }

        //Click handler for clicking the delete button
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            t.ToString();
            MenuPopup.IsOpen = false;

            //Make sure that we are allowed to delete, i.e. no references
            if (references == 0)
            {
                //Act according to what kind of popup this is
                switch (source)
                {
                    case "VARIABLE":
                        //No need to do anything special
                        break;
                    case "METHOD":
                        //Delete the additional method data
                        MainPage.Instance.deleteMethodAssets(list.IndexOf(deletionBlock));
                        break;
                    case "PARAMETER":
                        //Delete the additional parameter data
                        MainPage.Instance.deleteParameterAsset();
                        break;
                    default:
                        //Why did they add this?
                        Debug.WriteLine("how did you get here!?...get back I say!");
                        break;
                }

                //Remove the deleted block from the appropriate areas
                MainPage.nameList.Remove(deletionBlock.metadataList[1]);
                list.RemoveAt(list.IndexOf(deletionBlock));
            }
        }

        //Formats the popup for the deletion of an item (variable, method, etc.)
        public ObservableCollection<Block> DeletePopup(ObservableCollection<Block> list, Block delete)
        {
            this.list = list;
            deletionBlock = delete;

            //Hide unneeded items
            OkAddBtn.Visibility = Visibility.Collapsed;
            type.Visibility = Visibility.Collapsed;
            PopupComboBox.Visibility = Visibility.Collapsed;
            PopupTextBox.Visibility = Visibility.Collapsed;
            OkEditBtn.Visibility = Visibility.Visible;

            //Fill the references data
            getReferences(deletionBlock);
            PopupDeleteInfo.Text = "This item has " + references + " references";
            if (references > 0)
            {
                PopupDeleteInfo.Text += "\nRemove references first";
            }
            return this.list;
        }

        //Formats the popup for the creation of an item
        public void CreatePopup()
        {
            OkAddBtn.Visibility = Visibility.Visible;
            OkEditBtn.Visibility = Visibility.Collapsed;
            PopupDeleteInfo.Visibility = Visibility.Collapsed;
            DeleteConfirm.Visibility = Visibility.Collapsed;
        }

        //Returns the amount of refrences of the block b in the editors
        private void getReferences(Block b)
        {
            switch (source)
            {
                case "VARIABLE":
                    foreach (EditorDragDropTarget target in MainPage.editorLists)
                    {
                        references = references + SocketReader.checkCustoms(target.getTreeList(), b);
                    }
                    break;
                case "METHOD":
                    foreach (EditorDragDropTarget target in MainPage.editorLists)
                    {
                        references = references + SocketReader.checkCustoms(target.getTreeList(), b);
                    }
                    break;
                case "PARAMETER":
                    break;
                default:
                    Debug.WriteLine("invalid reference type");
                    references = -1;
                    break;
            }
            
        }

        public void SavePopup()
        {
            type.Visibility = Visibility.Collapsed;
            PopupComboBox.Visibility = Visibility.Collapsed;
            OkEditBtn.Visibility = Visibility.Collapsed;
            DeleteConfirm.Visibility = Visibility.Collapsed;
            PopupTextBox.Text = "Enter file name";
            OkAddBtn.Content = "Save";
            OkAddBtn.Click += new RoutedEventHandler(saveAdd_Click);
        }

        private void saveAdd_Click(object sender, RoutedEventArgs e)
        {
            string text = PopupTextBox.Text;
            MainPage.Instance.saveCode(text);
            MenuPopup.IsOpen = false;
        }
    }
}
