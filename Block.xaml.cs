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
using System.Diagnostics;
using ButtonControlLibrary;
using System.Collections.ObjectModel;

namespace CapGUI
{
    public partial class Block : UserControl
    {
        //public bool flag_hasSocks = false;
        //Actual name of the block (IF-Block, While-Block, etc.)
        private string blockName;
        public string Text { get { return blockName; } set { blockName = value; } }
        //Color the block is rendered in
        private Color privateColor;
        public Color blockColor// { get; set; }
        {
            get
            {
                return privateColor;
            }
            set
            {
                privateColor = value;
                LayoutRoot.Background = new SolidColorBrush(privateColor);
            }
        }

        //Block type border color definitions
        //public static Color statementBorderColor = Colors.Black;
        public static Color pluginBorderColor = Colors.Black;
        public static Color methodBorderColor = Colors.Green;
        public static Color errorBorderColor = Colors.Red;

        //ID of this type of block in the program, assigned by the reader. Each block in the palette gets a unique ID. 
        public int typeID { get; set; }
        //Index in the editor window
        public int index { get; set; }
        //Determines where the block can be placed-- STATEMENT, PLUGIN, STATEMENT/PLUGIN
        public string type { get; set; }
        
        //If this is a variable, method, or constant, the block's return type (STRING/INT/BOOLEAN)
        public string returnType { get; set; }

        //API Flags -- used to determine block behavior
        /*(not yet used)*/public bool flag_loopOnly { get; set; }                     //If true, this can only be socketed into a loop block 
        public bool flag_socketsMustMatch { get; set; }             //If true, all plugins must match each others' return types
        public bool flag_intDisabled { get; set; }                  //If true, plugins that return an INT can't be socketed here
        public bool flag_stringDisabled { get; set; }               //If true, plugins that return a STRING can't be socketed here
        public bool flag_booleanDisabled { get; set; }              //If true, plugins that return a BOOLEAN can't be socketed here
        public bool flag_requiresEndIf { get; set; }                //If true, this block must have a matching endIf and creates one on creation
        public bool flag_requiresEndLoop { get; set; }              //If true, this block must have a matching endLoop and creates one on creation
        public bool flag_requiresEndWait { get; set; }              //If true, this block must have a matching endWait and creates on on creation
        public bool flag_beginIndent { get; set; }                  //If true, blocks following this one are indented until an endIndent is met
        public bool flag_endIndent { get; set; }                    //If true, blocks preceding this one should be indented until this block is met
        public bool flag_followsIfOnly { get; set; }                //If true, this block can only follow an EndIf (i.e. this is an else block)
        /*(not yet used)*/public bool flag_allowInfinity { get; set; }                //If true, sockets can contain the INFINITY block
       /*(not yet used)*/ public bool flag_mustBePositive { get; set; }               //If true, INTs in this block must be positive (i.e. in loop conditions)
        public bool flag_isCondition { get; set; }                  //If true, this block is a conditional and can be used in conditional expressions (ands, ors, etc.)
        public bool flag_isConstant { get; set; }                   //If true, this block is a constant and part of the constant package
        public bool flag_isRobotConstant { get; set; }              //If true, this block is a constant and part of the robot constant package
        public bool flag_hasSocks { get; set; }                     //If true, this block contains at least 1 socket
        public bool flag_transformer { get; set; }                  //If true, block is more than meets the eye and will maximize when needed
        public bool flag_isCustom { get; set; }                     //If true, block is one that is customizable by the user
        public bool flag_printLiteral { get; set; }                 //If true, block should be parsed using toString instead of metadata

        public bool flag_isPackage { get; set; }                    //If true, this block is a marker for a package in the program palette
        public bool flag_programOnly { get; set; }                  //If true, this block can never be created by the user through the program palette (i.e. endifs, variables)
        public bool flag_robotOnly { get; set; }                    //If true, this block can never be created by the user through the robot palette (i.e. endifs, variables)

        //Holds the types / names of each of the fields in the block. (e.g. [String, Block, ...])
        public List<String> typeFieldList;
        //Holds the actual values of the fields defined by typeFieldList. (e.g. ["If", ConditionBlockInstance, ...])
        public List<Object> dataList;
        //Holds any metadata about that field-- restrictions and so on. (e.g. socket contents, textbox returns)
        public List<String> metadataList;

        public List<String> parameterList;

        public Block(string newBlockName, Color newBlockColor)
        {
            InitializeComponent();
            Text = newBlockName;
            blockColor = newBlockColor;
            LayoutRoot.Background = new SolidColorBrush(newBlockColor);
            fore.Text = String.Format("{0,-10}", Text);
            typeFieldList = new List<String>();
            dataList = new List<Object>();
            metadataList = new List<String>();

            parameterList = new List<String>();

            index = -1;
            initFlags();
            line.Text = String.Format("{0,-10}", "");
            //Debug.WriteLine(blockColor.ToString());
            blockBorder.BorderBrush = new SolidColorBrush(blockColor);

        }

        public Block(string newBlockName)
        {
            InitializeComponent();
            Text = newBlockName;
            blockColor = Colors.White;
            LayoutRoot.Background = new SolidColorBrush(blockColor);


            typeFieldList = new List<String>();
            dataList = new List<Object>();
            metadataList = new List<String>();

            parameterList = new List<String>();

            index = -1;
            initFlags();
            line.Text = String.Format("{0,-10}", "");
            fore.Text = String.Format("{0,-10}", Text);
            //Debug.WriteLine(blockColor.ToString());
            blockBorder.BorderBrush = new SolidColorBrush(blockColor);
        }

        /// <summary>
        /// Private method to default all flags to false
        /// </summary>
        private void initFlags()
        {
            flag_loopOnly = false;
            flag_socketsMustMatch = false;
            flag_intDisabled = false;
            flag_stringDisabled = false;
            flag_booleanDisabled = false;
            flag_requiresEndIf = false;
            flag_requiresEndLoop = false;
            flag_requiresEndWait = false;
            flag_beginIndent = false;
            flag_endIndent = false;
            flag_followsIfOnly = false;
            flag_allowInfinity = false;
            flag_mustBePositive = false;
            flag_isCondition = false;
            flag_isRobotConstant = false;
            flag_isConstant = false;
            flag_hasSocks = false;
            flag_transformer = false;
            flag_isCustom = false;
            flag_printLiteral = false;

            flag_isPackage = false;
            flag_programOnly = false;
            flag_robotOnly = false;
        }

        /// <summary>
        /// Appends a new field and data to the end of the block's definition.
        /// Use this when constructing a new instance of a block (in the editor window, etc.)
        /// </summary>
        /// Note that non-template blocks in the editor window can change dynamically-- adding/chaining conditionals, etc.
        /// <param name="newTypeField">Name of the data type (String, Block, etc.)</param>
        /// <param name="newData">Actual data to store in the block ("If", instance, etc.)</param>
        /// <param name="newMetaData">Any additional restrictions or metadata on the field</param>
        public void addField(string newTypeField, Object newData, string newMetaData)
        {
            typeFieldList.Add(newTypeField);
            dataList.Add(newData);
            metadataList.Add(newMetaData);
        }

        /// <summary>
        /// Appends a new field and data to the end of the block's definition.
        /// Use this when constructing block definitions (in blockPallete, etc.)
        /// </summary>
        /// <param name="newTypeField">Name of the data type (String, Block, etc.)</param>
        public void addField(string newTypeField)
        {
            typeFieldList.Add(newTypeField);
            dataList.Add(null);
            metadataList.Add(null);
        }

        /// <summary>
        /// Sets the data at a given index. (e.g. set the 'Condition' data field at index = 1 to a Block we provide)
        /// </summary>
        /// <param name="index">Index of the datafield to set</param>
        /// <param name="newData">Object to be set into the given index</param>
        public void setData(int index, object newData)
        {
            dataList[index] = newData;
        }

        public string returnName()
        {
            if (typeFieldList != null)
            {
                foreach (String s in typeFieldList)
                {
                    if (!s.Equals("socket"))
                        return metadataList[typeFieldList.IndexOf(s)];
                }
            }
            return null;
        }

        public void setLine(int index)
        {
            line.Text = String.Format("{0,-10}", String.Format("{0," + ((10 + index.ToString().Length) / 2).ToString() + "}", index.ToString()));
        }

        public override string ToString()
        {
            return Text;// +" Index: " + index;
        }

        public bool checkType(string type)
        {
            if (this.type != null)
            {
                if (this.type.Contains(type))
                    return true;
            }
            return false;
        }

        public void defineBorderColor()
        {
            //Define border color
            if (Text.Equals("METHOD"))
                blockBorder.BorderBrush = new SolidColorBrush(methodBorderColor);
            else if (type.Equals("PLUGIN"))
                blockBorder.BorderBrush = new SolidColorBrush(pluginBorderColor);
            else if (type.Equals("STATEMENT"))
                blockBorder.BorderBrush = new SolidColorBrush(blockColor);
            else
                blockBorder.BorderBrush = new SolidColorBrush(errorBorderColor);
        }

        //Returns a new copy of itself
        public Block cloneSelf(bool withSocket)
        {
            Block clone = new Block(Text, blockColor);
            //Copy all fields
            clone.type = type;
            clone.typeID = typeID;
            clone.returnType = returnType;

            clone.typeFieldList = new List<string>();
            clone.dataList = new List<object>();
            clone.metadataList = new List<string>();

            foreach (string s in typeFieldList)
                clone.typeFieldList.Add(s);
            foreach (object o in dataList)
                clone.dataList.Add(o);
            foreach (string s in metadataList)
                clone.metadataList.Add(s);

            //not sure if needed
            foreach (string s in parameterList)
                clone.parameterList.Add(s);

            clone.defineBorderColor();

            //Copy all flags (ugh)
            clone.flag_loopOnly = flag_loopOnly;
            clone.flag_socketsMustMatch = flag_socketsMustMatch;
            clone.flag_intDisabled = flag_intDisabled;
            clone.flag_stringDisabled = flag_stringDisabled;
            clone.flag_booleanDisabled = flag_booleanDisabled;
            clone.flag_requiresEndIf = flag_requiresEndIf;
            clone.flag_requiresEndLoop = flag_requiresEndLoop;
            clone.flag_requiresEndWait = flag_requiresEndWait;
            clone.flag_beginIndent = flag_beginIndent;
            clone.flag_endIndent = flag_endIndent;
            clone.flag_followsIfOnly = flag_followsIfOnly;
            clone.flag_allowInfinity = flag_allowInfinity;
            clone.flag_mustBePositive = flag_mustBePositive;
            clone.flag_isCondition = flag_isCondition;
            clone.flag_isRobotConstant = flag_isRobotConstant;
            clone.flag_isConstant = flag_isConstant;
            clone.flag_hasSocks = flag_hasSocks;
            clone.flag_transformer = flag_transformer;
            clone.flag_isCustom = flag_isCustom;
            clone.flag_printLiteral = flag_printLiteral;

            clone.flag_isPackage = flag_isPackage;
            clone.flag_programOnly = flag_programOnly;
            clone.flag_robotOnly = flag_robotOnly;

            if (withSocket)
                clone.fieldCreation();
            else
                clone.nonSocketFieldCreation();

            return clone;
        }

        public void fieldCreation()
        {
            innerPane.Children.Remove(fore);
            int socketCTR = 0;
            bool multiple = false;
            foreach (String s in typeFieldList)
            {
                if (s.Equals("socket"))
                    socketCTR++;
            }
            if (socketCTR > 1)
                multiple = true;
            for (int i = 0; i < typeFieldList.Count; i++)
            {

                switch (typeFieldList[i])
                {
                    case "string":
                        createText(metadataList[i]);
                        break;
                    case "socket":
                        createSocket(metadataList[i], multiple);
                        break;
                    case "textbox":
                        createTextBox(metadataList[i]);
                        break;
                    case "dropdown":
                        createDropDown();
                        break;
                    case "parameterList":
                        createParameterList();
                        break;
                    default:
                        //Debug.WriteLine("Field not found: " + typeFieldList[i]);
                        break;
                }
            }
            if(Text.Contains("ASSIGN"))
            {
                adjustAssign();
            }
        }

        public void nonSocketFieldCreation()
        {
            innerPane.Children.Remove(fore);
            for (int i = 0; i < typeFieldList.Count; i++)
            {

                switch (typeFieldList[i])
                {
                    case "string":
                        createText(metadataList[i]);
                        break;
                    case "textbox":
                        createTextBox(metadataList[i]);
                        break;
                    default:
                        //Debug.WriteLine("Not printing sockets");
                        break;
                }
            }
        }

        private void createText(string info)
        {
            TextBlock text = new TextBlock();
            text.Text = String.Format("{0,-10}", String.Format("{0," + ((10 + info.Length) / 2).ToString() + "}", info));
            text.FontSize = 13;
            innerPane.Children.Add(text);
        }

        private void createTextBox(string type)
        {
            //creating a style to eliminate extra whitespace
            Style style = new Style(typeof(TextBox));
            style.Setters.Add(new Setter(TextBox.PaddingProperty, new Thickness(-1)));
            //flag_hasSocks = true;
            switch (type)
            {
                case "INT":
                    //NumericTextBox numTextBox = new NumericTextBox();
                    TextBox numTextBox = new TextBox();
                    numTextBox.Width = 100;
                    numTextBox.Height = LayoutRoot.Height - 19;
                    numTextBox.Style = style;
                    innerPane.Children.Add(numTextBox);
                    break;
                case "STRING":
                    TextBox textBox = new TextBox();
                    textBox.Width = 100;
                    textBox.Height = LayoutRoot.Height - 15;
                    textBox.Style = style;
                    innerPane.Children.Add(textBox);
                    break;
                default:
                    Debug.WriteLine("error");
                    break;
            }
        }

        private void createDropDown()
        {
            ComboBox cb = new ComboBox();
            for(int i = 0; i < typeFieldList.Count; i++)
            {
                if (typeFieldList[i].Equals("field"))
                {
                    //could be done in one line.  Coded this way for readablility//
                    ComboBoxItem item = new ComboBoxItem();
                    TextBlock text = new TextBlock();
                    text.Text = metadataList[i];
                    item.Content = text;
                    cb.Items.Add(item);
                    ////////////////////
                }
            }
            cb.SelectedIndex = 0;
            innerPane.Children.Add(cb);
        }

        private void createParameterList()
        {
            flag_hasSocks = false;
            StackPanel webby2 = new StackPanel();
            webby2.Orientation = Orientation.Vertical;
            innerPane.Children.Add(webby2);
            for (int i = 0; i < parameterList.Count; i++)
            {
                flag_hasSocks = true;
                SocketDragDropTarget target = new SocketDragDropTarget();
                target.socketType = parameterList[i];

                Style style = new Style(typeof(ListBoxItem));
                style.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(-1)));

                if (type.Contains("CONDITION"))
                    target.isCondition = true;
                if (type.Contains("CONSTANT"))
                    target.isConstant = true;

                ListBox listBox = new ListBox();
                ObservableCollection<Block> collection = new ObservableCollection<Block>();
                listBox.Height = 40;
                listBox.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
                listBox.ItemContainerStyle = style;
                listBox.Width = LayoutRoot.Width * .6;
                listBox.BorderThickness = new Thickness(2);
                listBox.Background = new SolidColorBrush(Colors.LightGray);
                listBox.ItemsSource = collection;
                target.Content = listBox;
                if(i > 0)
                    LayoutRoot.Height = LayoutRoot.Height + listBox.Height;
                webby2.Children.Add(target);
            }
        }

        //Currently creates numeric text boxes
        public void createSocket(string type, bool multiple)
        {
            SocketDragDropTarget target = new SocketDragDropTarget();
            target.socketType = type;

            Style style = new Style(typeof(ListBoxItem));
            style.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(-1)));

            if (type.Contains("CONDITION"))
                target.isCondition = true;
            if (type.Contains("CONSTANT"))
                target.isConstant = true;

            ListBox listBox = new ListBox();
            listBox.Height = LayoutRoot.Height - 2;
            listBox.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
            listBox.ItemContainerStyle = style;
            /*if (multiple)
            {
                listBox.Width = LayoutRoot.Width * .6;
                //listBox.MinWidth = LayoutRoot.Width * .75;

                //listBox.Width = LayoutRoot.Width * .33;//175;
            }
            else*/
                listBox.Width = LayoutRoot.Width * .75;//350;

            listBox.BorderThickness = new Thickness(2);
            listBox.Background = new SolidColorBrush(Colors.LightGray);
            target.Content = listBox;
            innerPane.Children.Add(target);
        }

        //adjusting assignment block into stacked structure
        private void adjustAssign()
        {
            LayoutRoot.Height *= 2.4;
            List<System.Windows.UIElement> components = innerPane.Children.ToList();
            innerPane.Children.Clear();

            //making sure line number and name are not in the stackpanel
            innerPane.Children.Add(components.ElementAt(0));
            components.RemoveAt(0);
            innerPane.Children.Add(components.ElementAt(0));
            components.RemoveAt(0);

            StackPanel stackBox = new StackPanel();
            stackBox.Width = LayoutRoot.Width *.75;
            stackBox.MaxWidth = LayoutRoot.Width *.75;

            foreach (System.Windows.UIElement element in components)
            {
                //casting sockets into specific objects so their widths can be changed
                if (element is SocketDragDropTarget)
                {
                    SocketDragDropTarget x = (SocketDragDropTarget)element;
                    x.Width = stackBox.Width -2;
                    ListBox inner = (ListBox)x.Content;
                    inner.Width = stackBox.Width -2;
                    stackBox.Children.Add(x);
                }
                else
                {
                    stackBox.Children.Add(element);
                }
            }

            //style to eliminate excess padding
            Style style = new Style(typeof(StackPanel));
            style.Setters.Add(new Setter(StackPanel.MarginProperty, new Thickness(-1)));
            stackBox.Style = style;

            //setting up elements in order to hide whitespace
            SolidColorBrush tempBrush = new SolidColorBrush(Colors.Purple);
            stackBox.Background = tempBrush;
            stackBox.Background.Opacity = 0;

            innerPane.Children.Add(stackBox);
        }
    }
}
