using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Linq;
using CapGUI.Parsing;

namespace CapGUI.xml
{
    public class LoadXML
    {

        private static int methodIndex = 0;

        public static Dictionary<string, int> blockLookUp = new Dictionary<string, int>();
        
        public static void LoadFromXML()
        {
            clearEditors();
            clearVariables();
            clearMethods();

            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            if (iso.FileExists("save.xml"))
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("save.xml", FileMode.Open, iso))
                {
                    XDocument apiDoc = XDocument.Load(isoStream);
                    IEnumerable<XElement> fullAPI = apiDoc.Elements();
                    IEnumerable<XElement> groups = fullAPI.Elements();
                    foreach (var group in groups)
                    {
                        String groupName = group.Attribute("name").Value.ToString();
                        //Debug.WriteLine(groupName);
                        switch (groupName)
                        {
                            case "VARIABLES":
                                addVariables(group);
                                break;
                            case "METHODS":
                                addMethods(group);
                                break;
                            case "EDITORS":
                                addEditors(group);
                                break;
                            default:
                                Debug.WriteLine("Not in group");
                                break;
                        }     
                    }
                }
            }
        }

        /*
         * Clear All the data from the ListBoxs in all Editors
         */
        private static void clearEditors()
        {
            if (MainPage.editorLists != null)
            {
                foreach(EditorDragDropTarget EDDT in MainPage.editorLists)
                {
                    //get the listbox of each editor and clear the content
                    MainPage.editorLists[MainPage.editorLists.IndexOf(EDDT)].clearTree(MainPage.editorLists[MainPage.editorLists.IndexOf(EDDT)].Content as ListBox);
                }
            }
        }

        /*
         * Clear All Variables
         */
        private static void clearVariables()
        {
            for (int i = MainPage.variableList.Count - 1; i >= 0; i--)
            {
                MainPage.nameList.RemoveAt(i);
                MainPage.variableList.RemoveAt(i);
            }
        }

        /*
         * Clear All Methods
         */
        private static void clearMethods()
        {
            for (int i = MainPage.methodList.Count - 1; i >= 0; i--)
            {
                MainPage.Instance.deleteMethodAssets(i);
                MainPage.nameList.RemoveAt(i);
                MainPage.methodList.RemoveAt(i);
            }
        }

        private static void addVariables(XElement varaibles)
        {
            IEnumerable<XElement> vars = varaibles.Elements();
            foreach (var variable in vars)
            {
                string name = variable.Element("name").Value;
                string type = variable.Element("return").Value;
                MainPage.nameList.Add(name);
                MainPage.Instance.createVariable(name, type);
            }
        }

        private static void addMethods(XElement methods)
        {
            IEnumerable<XElement> meths = methods.Elements();
            foreach (var method in meths)
            {
                string name = method.Element("name").Value;
                string type = method.Element("return").Value;
                MainPage.nameList.Add(name);
                MainPage.Instance.createMethod(name, type);
                IEnumerable<XElement> parameters = method.Element("PARAMETERS").Elements();
                foreach (var parameter in parameters)
                {
                    string paramName = parameter.Element("name").Value;
                    string paramType = parameter.Element("return").Value;
                    foreach (TabItem tab in MainPage.Instance.editorTabControl.Items)
                    {
                        //Debug.WriteLine(tab.Name);
                        if (tab.Name.Equals(name + "Tab"))
                        {
                            TabPage page = tab.Content as TabPage;
                            MainPage.nameList.Add(paramName);
                            page.createParameter(paramName, paramType);
                        }
                    }
                }
            }
        }

        private static void addEditors(XElement editors)
        {
            IEnumerable<XElement> editor = editors.Elements();
            foreach (var midnight in editor) //time of great programming
            {
                string editorName = midnight.Attribute("name").Value.ToString();
                EditorDragDropTarget EDDT = null;
                //get a refrence to the EDDT for the current midnight (redundant)
                foreach (EditorDragDropTarget target in MainPage.editorLists) //ask team to find a better way to do this
                {
                    if(target.Name.Equals(editorName))
                    {
                        //Debug.WriteLine(target.Name);
                        EDDT = MainPage.editorLists[MainPage.editorLists.IndexOf(target)];
                        break;
                    }
                }

                addBlocks(midnight, EDDT);
                methodIndex++;
            }
        }

        private static void addBlocks(XElement editor, EditorDragDropTarget EDDT)
        {
            IEnumerable<XElement> blocks = editor.Elements();
            foreach (var block in blocks)
            {
                int blockID = blockLookUp[block.Attribute("type").Value];
                string blockName = block.Attribute("type").Value;
                int line = Int32.Parse(block.Element("LINE").Value);
                Debug.WriteLine(blockName + " ID: " + blockID + " Line: " + line);
                if (!blockName.Contains("END"))
                {
                    Block copyBlock = MainPage.createProgramStructureBlock(blockID);
                    Block endBlock = null;

                    //currently adding so that program doesn't break.
                    //need to add correct block in this case methods by name.
                    if (copyBlock == null)
                    {
                        switch (blockName)
                        {
                            case "METHOD":
                                foreach (Block b in MainPage.methodList) //find method block
                                {
                                    if (b.metadataList[1].Equals(block.Element("name").Value))
                                    {
                                        copyBlock = b.cloneSelf(true);
                                        break;
                                    }
                                }
                                break;
                            case "RETURN":
                                TabPage methodCode = (TabPage)MainPage.tabList[methodIndex-1].Content; //MainPage.methodList.IndexOf(b)
                                copyBlock = methodCode.getReturnBlock();
                                if(copyBlock == null)
                                    copyBlock = MainPage.createReservedBlock(blockName);
                                break;
                            default:
                                copyBlock = MainPage.createReservedBlock(blockName);
                                break;
                        }       
                    }

                    //create end blocks for those who neeed it
                    if (blockName.Contains("IF"))
                    {
                        endBlock = MainPage.createReservedBlock("ENDIF");
                    }
                    else if (blockName.Equals("ELSE"))
                    {
                        endBlock = MainPage.createReservedBlock("ENDELSE");
                    }
                    else if (copyBlock.flag_requiresEndLoop)
                    {
                        endBlock = MainPage.createReservedBlock("ENDLOOP");
                    }

                    
                    //add block and call sockets to be added
                    if (endBlock == null)
                    {
                        EDDT.addNodeToTree(copyBlock, line);                       
                    }
                    else
                    {
                        EDDT.addNodeToTree(copyBlock, endBlock, line);
                    }
                    
                    //List<int> socketPosition = SocketReader.socketFinder(copyBlock);
                    //foreach (int socket in socketPosition)
                    //{
                     //   Debug.WriteLine("socket location: " + socket);
                        getSocket(block, copyBlock);
                    //}
                }
            }
            EDDT.setTree(EDDT.Content as ListBox);
        }

        private static void getSocket(XElement blocks, Block parent)
        {
            IEnumerable<XElement> sockets = blocks.Elements("SOCKET");
            List<int> socketPosition = SocketReader.socketFinder(parent);
            int index = 0;
            foreach (var socket in sockets)
            {
                if (socketPosition.Count > 0)
                {
                    addSocketBlock(socket, parent, socketPosition[index]);
                    index++;
                }
            }
        }

        private static void addSocketBlock(XElement socket, Block parent, int position)
        {
            IEnumerable<XElement> blocks = socket.Elements();
            foreach (var block in blocks)
            {
                int blockID = blockLookUp[block.Attribute("type").Value];
                string blockName = block.Attribute("type").Value;
                Debug.WriteLine(blockName + " ID: " + blockID);
                if (!blockName.Contains("END"))
                {
                    Block copyBlock = MainPage.createProgramStructureBlock(blockID);

                    //currently adding so that program doesn't break.
                    //need to add correct block in this case methods by name.
                    if (copyBlock == null)
                    {
                        copyBlock = MainPage.createReservedBlock(blockName);
                        switch (blockName)
                        {
                            case "VARIABLE":
                                foreach (Block b in MainPage.variableList) //find method block
                                {
                                    if (b.metadataList[1].Equals(block.Element("name").Value))
                                    {
                                        copyBlock = b.cloneSelf(true);
                                        break;
                                    }
                                }
                                break;
                            case "METHOD":
                                foreach (Block b in MainPage.methodList) //find method block
                                {
                                    if (b.metadataList[1].Equals(block.Element("name").Value))
                                    {
                                        copyBlock = b.cloneSelf(true);
                                        break;
                                    }
                                }
                                break;
                            case "PARAMETER":
                                TabPage methodCode = (TabPage)MainPage.tabList[methodIndex-1].Content; //MainPage.methodList.IndexOf(b)
                                //Debug.WriteLine(methodCode.getParamForName(block.Element("name").Value));
                                copyBlock = methodCode.getParamForName(block.Element("name").Value);
                                //saftey in case name was not written correctly
                                if(copyBlock == null)
                                    copyBlock = MainPage.createReservedBlock(blockName);
                                break;
                            default:
                                Debug.WriteLine("type not defined");
                                break;
                        }
                    }
                    else if ((copyBlock.flag_isConstant || copyBlock.flag_isRobotConstant) && !copyBlock.ToString().Contains("RANGE"))
                    {
                        if (copyBlock.flag_hasSocks)
                        {
                            int slot = SocketReader.textFinder(copyBlock);
                            List<System.Windows.UIElement> components = copyBlock.innerPane.Children.ToList();
                            if (components.ElementAt(slot) is NumericTextBox)//.ToString().Equals("CapGUI.NumericTextBox") || components.ElementAt(location).ToString().Equals("System.Windows.Controls.TextBox"))
                            {
                                NumericTextBox box = (NumericTextBox)components.ElementAt(slot);
                                box.Text = block.Element("VALUE").Value;
                            }
                            else if (components.ElementAt(slot) is TextBox)
                            {
                                TextBox box = (TextBox)components.ElementAt(slot);
                                box.Text = block.Element("VALUE").Value;
                            }
                        }
                        else if (copyBlock.Text.Equals("GETBEARING") || copyBlock.Text.Equals("INFINITY")) //getBearing and infinity both dont have any metadata
                        {
                        }
                        else
                        {
                            ComboBox cb = (ComboBox)copyBlock.innerPane.Children.ElementAt(2);
                            directionalCombos(parent, copyBlock);
                            cb.SelectedIndex = int.Parse(block.Element("INDEX").Value);
                        }
                    }

                    AddToSocket(parent, position, copyBlock);
                    getSocket(block, copyBlock);
                }
            }
        }

        //method to change combo boxes in directional blocks
        private static void directionalCombos(Block Parent, Block copyBlock)
        {
            ComboBox cb = (ComboBox)copyBlock.innerPane.Children.ElementAt(2);

            //if turn, remove front, rear, backwards and forwards
            if (Parent.ToString().Contains("TURN"))
            {
                Debug.WriteLine(copyBlock.innerPane.Children.ElementAt(2));
                cb.Items.RemoveAt(3);
                cb.Items.RemoveAt(2);
                cb.Items.RemoveAt(1);
                cb.Items.RemoveAt(0);
                cb.SelectedItem = cb.Items.ElementAt(0);
            }
            //if drive, remove front,rear, left and right
            else if (Parent.ToString().Contains("DRIVE"))
            {
                Debug.WriteLine(copyBlock.innerPane.Children.ElementAt(2));
                cb.Items.RemoveAt(5);
                cb.Items.RemoveAt(4);
                cb.Items.RemoveAt(3);
                cb.Items.RemoveAt(2);
            }
            //if distance, remove backwards and forwards
            else if (Parent.ToString().Contains("RANGE"))
            {
                Debug.WriteLine(copyBlock.innerPane.Children.ElementAt(2));
                cb.Items.RemoveAt(1);
                cb.Items.RemoveAt(0);
                cb.SelectedIndex = 0;
            }
        }

        public static void AddToSocket(Block source, int i, Block child)
        {
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
                //checking if in main panel or socket
                if (source.innerPane.Children.Count == 4)
                {
                    StackPanel innards = (StackPanel)source.innerPane.Children.ElementAt(3);
                    components = innards.Children.ToList();
                }
                else
                {
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
            else
            {
                components = source.innerPane.Children.ToList();
            }

            SocketDragDropTarget SDDT = (SocketDragDropTarget)components.ElementAt(i);
            ListBox listBox = (ListBox)SDDT.Content;
            SDDT.ResizeAndAdd(listBox, child);
        }
    }
}
