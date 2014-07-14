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

namespace CapGUI
{
    public partial class PopupInterface : UserControl
    {
        private ObservableCollection<Block> list;
        private string source;
        private int references = 0;
        private Block deletionBlock;

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

            if (MainPage.Instance.lessons != null)
            {
                for (int i = 0; i < MainPage.Instance.lessons.Count; i++)
                {
                    PopupComboBox.Items.Add(((new ComboBoxItem()).Content = MainPage.Instance.lessons[i]));
                }
            }

            int selectedIndex = PopupComboBox.Items.IndexOf(currentItem);
            PopupComboBox.SelectedIndex = selectedIndex;

            OkAddBtn.Content = "OK";

        }

        private void Child_OnHasFocus(object sender, RoutedEventArgs e)
        {
            PopupTextBox.IsReadOnly = false;
            PopupTextBox.Text = "";
        }

        private void variableCancel_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
        }

        private void Child_OnLostFocus(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            if (references == 0)
            {
                switch (source)
                {
                    case "VARIABLE":
                        break;
                    case "METHOD":
                        MainPage.Instance.deleteMethodAssets(list.IndexOf(deletionBlock));
                        break;
                    case "PARAMETER":
                        MainPage.Instance.deleteParameterAsset();
                        break;
                    default:
                        Debug.WriteLine("how did you get here!?...get back I say!");
                        break;
                }
                MainPage.nameList.Remove(deletionBlock.metadataList[1]);
                list.RemoveAt(list.IndexOf(deletionBlock));
            }
        }

        

        public ObservableCollection<Block> DeletePopup(ObservableCollection<Block> list, Block delete)
        {
            this.list = list;
            deletionBlock = delete;
            OkAddBtn.Visibility = Visibility.Collapsed;
            type.Visibility = Visibility.Collapsed;
            PopupComboBox.Visibility = Visibility.Collapsed;
            PopupTextBox.Visibility = Visibility.Collapsed;
            OkEditBtn.Visibility = Visibility.Visible;
            getReferences(deletionBlock);
            PopupDeleteInfo.Text = "This item has " + references + " references";
            if (references > 0)
            {
                PopupDeleteInfo.Text += "\nRemove references first";
            }
            return this.list;
        }

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
    }
}
