using System;
using System.Collections.ObjectModel;
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
using System.Xml;
using CapGUI.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace CapGUI.xml
{
    public class SaveXML
    {
        public static void SaveToXML()
        {
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            if (!iso.FileExists("save.xml"))
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("save.xml", FileMode.CreateNew, iso))
                {
                    // Write to the Isolated Storage for the user.
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    // Create an XmlWriter.
                    using (XmlWriter writer = XmlWriter.Create(isoStream, settings))
                    {
                        writer.WriteStartElement("SaveFile");
                        #region Variables
                        writer.WriteComment("Variables?");

                        // Write the Variable root
                        writer.WriteStartElement("GROUP");
                        writer.WriteAttributeString("name","VARIABLES");

                        // Write all Variables
                        foreach (Block b in MainPage.variableList)
                        {
                            writer.WriteStartElement("VARIABLE");
                            writer.WriteStartElement("name"); 
                            writer.WriteString(b.metadataList[1]); //name of variable
                            writer.WriteEndElement();
                            writer.WriteStartElement("return");
                            writer.WriteString(b.metadataList[0]); //return type
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                        }

                        // Write the close tag for Variables
                        writer.WriteEndElement();
                        #endregion

                        #region Methods
                        writer.WriteComment("Methods?");

                        // Write the Method root
                        writer.WriteStartElement("GROUP");
                        writer.WriteAttributeString("name", "METHODS");

                        // Write all Methods
                        foreach (Block b in MainPage.methodList)
                        {
                            writer.WriteStartElement("METHOD");
                            writer.WriteStartElement("name");
                            writer.WriteString(b.metadataList[1]); //name of method
                            writer.WriteEndElement();
                            writer.WriteStartElement("return");
                            writer.WriteString(b.returnType); //return type
                            writer.WriteEndElement();
                            writer.WriteStartElement("PARAMETERS");
                            TabPage methodCode = (TabPage)MainPage.tabList[MainPage.methodList.IndexOf(b)].Content;
                            for (int i = 0; i < methodCode.parameterBox.Items.Count; i++)
                            {
                                writer.WriteStartElement("PARAMETER");
                                Block param = methodCode.getParamAtIndex(i);
                                writer.WriteStartElement("name");
                                writer.WriteString(param.metadataList[1]); //name of parameter
                                writer.WriteEndElement();
                                writer.WriteStartElement("return");
                                writer.WriteString(param.metadataList[0]); //return type
                                writer.WriteEndElement();
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                        }

                        // Write the close tag for Methods
                        writer.WriteEndElement();
                        #endregion

                        #region Editors
                        writer.WriteComment("Editors?");

                        // Write the EDITOR root
                        writer.WriteStartElement("GROUP");
                        writer.WriteAttributeString("name", "EDITORS");

                        // Write all EDDT's
                        foreach (EditorDragDropTarget EDDT in MainPage.editorLists)
                        {
                            ReadOnlyObservableCollection<Block> collection = EDDT.getTreeList(); //get tree list
                            writer.WriteStartElement("EDITOR");
                            writer.WriteAttributeString("name", EDDT.Name);
                            foreach (Block b in collection)
                            {
                                writer.WriteStartElement("BLOCK");
                                writer.WriteAttributeString("type", b.Text); //type of block
                                writer.WriteStartElement("LINE");
                                writer.WriteString(collection.IndexOf(b).ToString()); //line number of block
                                writer.WriteEndElement();
                                if (b.Text.Equals("METHOD"))
                                {
                                    writer.WriteStartElement("name");
                                    writer.WriteString(b.metadataList[1]); //name of method
                                    writer.WriteEndElement();
                                }
                                if(b.flag_hasSocks) //if there are sockets we need to dive in
                                {
                                    WriteSockets(writer, b);
                                }
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                        }

                        // Write the close tag for Variables
                        writer.WriteEndElement();
                        #endregion
                        writer.WriteEndElement();
                        // Write the XML to the file.
                        writer.Flush();
                    }
                }
            }
        }

        //method to write the socket xml data
        private static void WriteSockets(XmlWriter writer, Block b)
        {
            
            List<int> socketList = SocketReader.socketFinder(b);

            //writing data from text boxes and combo boxes
            if((b.flag_isConstant || b.flag_isRobotConstant )&& !b.ToString().Contains("RANGE"))
            {

                //handling basic text
                if (b.flag_hasSocks )
                {
                    int slot = SocketReader.textFinder(b);
                    writer.WriteStartElement("VALUE");
                    writer.WriteString(SocketReader.textReader(b, slot));
                    writer.WriteEndElement();
                }
                else if (b.Text.Equals("GETBEARING") || b.Text.Equals("INFINITY")) //getBearing and infinity both dont have any metadata
                {
                }
                    //handling combo boxes
                else
                {
                    ComboBox cb = (ComboBox)b.innerPane.Children.ElementAt(1);
                    writer.WriteStartElement("INDEX");
                    writer.WriteString(cb.SelectedIndex.ToString());
                    writer.WriteEndElement();
                }

            }
                //handling standard sockets
            else
            {

                foreach (int location in socketList)
                {
                    writer.WriteStartElement("SOCKET");

                    ListBox socket = SocketReader.socketMole(b, location);

                    //ensuring the socket isn't empty
                    if (socket.Items.Count > 0)
                    {
                        Block infosphere = (Block)socket.Items.ElementAt(0);

                        //checking if block is a variable or method
                        if (infosphere.flag_isCustom)
                        {
                            //if (infosphere.Name.Equals("VARIABLE"))
                            // {
                            writer.WriteStartElement("BLOCK");
                            writer.WriteAttributeString("type", infosphere.Text); //type of block
                            //    writer.WriteStartElement("VARIABLE");
                            //}
                            // else if (infosphere.Name.Equals("METHOD"))
                            // {
                            //    writer.WriteStartElement("METHOD");
                            // }
                            writer.WriteStartElement("name");
                            writer.WriteString(infosphere.metadataList[1]); //name of variable
                            writer.WriteEndElement();
                        }
                        else
                        {
                            writer.WriteStartElement("BLOCK");
                            writer.WriteAttributeString("type", infosphere.Text); //type of block
                            //writer.WriteStartElement(infosphere.ToString());
                        }
                        WriteSockets(writer, infosphere);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }
        }
    }
}
