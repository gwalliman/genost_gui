using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;

namespace CapGUI.Parsing
{
    public class CodeParser
    {
        private static List<String> indentCount = new List<String>();

        public CodeParser()
        {
        }

        public static void parseCode(EditorDragDropTarget page)
        {
            ReadOnlyObservableCollection<Block> list = page.getTreeList();

            foreach (Block b in list)
            {
                string s = "";         
                if (b.flag_endIndent)
                {
                    indent(false);
                    foreach (String str in indentCount)
                        s = s + str;
                    s += "}";
                    writeToFile(s);
                }
                else
                {
                    foreach (String str in indentCount)
                        s = s + str;
                    //checks to see if the block should use name or contents to print into code
                    if(b.flag_printLiteral)
                    {
                        s += (b.ToString()).ToLower() + SocketReader.readSocket(b);
                        //s = s.ToLower();
                    }
                    else if (b.flag_robotOnly)
                    {
                        s += SocketReader.translateRobotFunctions(b) + ";";
                    }
                    else if (b.flag_isCustom && b.flag_transformer)
                    {
                        s += (b.metadataList[0]).ToLower() + " " + b.metadataList[1];
                        s += SocketReader.readSocket(b);
                        if (s[s.Length - 1] != ')')
                        {
                            s += " ()";
                        }
                        s += ";";
                    }
                    else
                    {
                        s += (b.metadataList[0]).ToLower();
                        s += SocketReader.readSocket(b);
                    }

                    if (b.metadataList[0].Equals("ASSIGN") || b.metadataList[0].Equals("WAIT UNTIL") || b.metadataList[0].Equals("RETURN"))
                    {
                        s += ";";
                    }

                    writeToFile(s);
                    if (b.flag_beginIndent)
                    {
                        s = "";
                        foreach (String str in indentCount)
                            s = s + str;
                        s += "{";
                        writeToFile(s);
                        indent(true);
                    }
                }
            }
            indent(false);
        }

        public static void parseVariable(ObservableCollection<Block> variables, EditorDragDropTarget page)
        {
            ReadOnlyObservableCollection<Block> list = page.getTreeList();
            indent(true);

            foreach (Block b in variables)
            {
                string s = "";
                foreach (String str in indentCount)
                    s = s + str;
                s = s + "vardecl";
                foreach (string str in b.metadataList)
                {
                    //catching caps in var declaration and setting them to lowercase
                    if (str.Contains("INT") || str.Contains("BOOL") || str.Contains("STRING"))
                    {
                        s += " " + str.ToLower();
                    }
                    else
                    {
                        s = s + " " + str;
                    }
                }
                s = s + ";";
                if (SocketReader.checkCustoms(list, b) > 0)
                {
                    writeToFile(s);
                }
            }
        }

        //I don't get this method and it doesn't work how I expected, thanks for putting in the effort for some damn comments in your code David...
        private static void indent(bool begin)
        {
            if (begin)
                indentCount.Add("\t");
            else if (indentCount.Count > 0)
                indentCount.RemoveAt(0);
        }

        public static void writeToFile(string s)
        {
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            if (!iso.FileExists("test.txt"))
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("test.txt", FileMode.CreateNew, iso))
                {
                    using (StreamWriter sw = new StreamWriter(isoStream))
                    {
                        sw.WriteLine(s);
                    }
                }
            }
            else
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("test.txt", FileMode.Append, iso))
                {                   
                    using (StreamWriter sw = new StreamWriter(isoStream))
                    {
                        sw.WriteLine(s);
                    }
                }
            }
        }

        //method to read method tabs and write them to the code file
        public static void parseMethods(ObservableCollection<Block> methodList,List<TabItem> tabList)
        {
            TabPage methodCode;
            string methodHeader;
            Block param;

            foreach(TabItem TI in tabList)
            {
                methodCode = (TabPage)TI.Content;
                methodHeader = "\tmethoddefine ";

                //if tab hasn't been opened, this causes a crash due to null reference. Now defaults it to void return type...though only an idiot would use an empty method, so this shouldn't be needed...
                try
                {
                    methodHeader += ((TextBlock)methodCode.returnType.SelectionBoxItem).Text.ToLower() + " ";//returnType
                }
                catch (NullReferenceException)
                {
                    methodHeader += "void ";
                }

                methodHeader += TI.Name.Substring(0, TI.Name.Length - 3);
                methodHeader += "(";

                //individually adding parameter information
                for (int i = 0; i < methodCode.parameterBox.Items.Count; i++)
                {
                    param = methodCode.getParamAtIndex(i);
                    methodHeader += param.metadataList[0].ToLower() + " " + param.metadataList[1];
                    //adding commas, when needed
                    if (i != methodCode.parameterBox.Items.Count -1)
                    {
                        methodHeader += ", ";
                    }
                }
                methodHeader += ")";
                writeToFile(methodHeader);
                writeToFile("\t{");

                indent(true);
                parseVariable(MainPage.variableList, methodCode.tempDragDrop);
                parseCode(methodCode.tempDragDrop);
                indent(false);
                
                
                writeToFile("\t}\r");
                
            }
        }
    }
}
