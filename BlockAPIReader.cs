using System;
using System.Collections.Generic;
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
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CapGUI
{
    /// <summary>
    /// Reads in blocks from the API file provided (XML, etc.) and creates lists of block definitions in our system
    /// </summary>
    public class BlockAPIReader
    {

        //List of names for each package read in
        private List<String> packageNames;

        private IEnumerable<XElement> xmlDoc = null;
        private bool flag_fromServer = false;
        
        //List of Lists of blocks contained in each package (e.g. index 0=list of blocks in package0)
        List<List<Block>> blockList;
        //List of blocks that are program reserved
        List<Block> reservedBlockList;
        //Used to assign IDs to blocks 
        private int IDCounter = 0;

        //associated maze ID
        private string mazeID;
        
        public BlockAPIReader()
        {
            packageNames = new List<String>();
            reservedBlockList = new List<Block>();
            IDCounter = 0;                   //reset id counter
            xml.LoadXML.blockLookUp.Clear(); //clear id in Dictionary
        }

        public BlockAPIReader(IEnumerable<XElement> xmlFile)
        {
            packageNames = new List<String>();
            reservedBlockList = new List<Block>();
            IDCounter = 0;                   //reset id counter
            xml.LoadXML.blockLookUp.Clear(); //clear id in Dictionary
            flag_fromServer = true;          //tell readBlockDefinitoins that it is loading from a server file
            xmlDoc = xmlFile;

        }


        /// <summary>
        /// Reads in blocks from an API file.
        /// </summary>
        /// <returns>List of block packages, each containing a list of blocks</returns>
        public List<List<Block>> readBlockDefinitions()
        {
            blockList = new List<List<Block>>();
            try
            {
                //Get and create the XML document to read from
                IEnumerable<XElement> fullAPI = null;
                if (flag_fromServer)
                {
                    //Get an enumerator for each package node
                    fullAPI = xmlDoc;
                    //set flag back to false
                    flag_fromServer = false;
                }
                else
                {
                    XDocument apiDoc = XDocument.Load("xml/blockAPI.xml");//xmlDoc;
                    //Get an enumerator for each package node
                    fullAPI = apiDoc.Elements();
                }
                Debug.WriteLine(fullAPI);
                IEnumerable<XElement> packages = fullAPI.Elements();

                //gets the maze identifier...for some reason this doesn't seem to want to work without the foreach loop, not sure why there appears to be no single element targetting...
               /* foreach (var mazeid in packages)
                {
                    mazeID = mazeid.Value.ToString();
                }
                Debug.WriteLine(mazeID);*/

                foreach (var package in packages)
                {
                    if (package.Name.ToString().Equals("mazeid"))
                    {
                        mazeID = package.Value.ToString();
                        break;
                    }
                    //Gets each package
                    String pkgName = package.Attribute("name").Value.ToString();

                    packageNames.Add(pkgName);
                    
                    IEnumerable<XElement> blocks = package.Elements();
                    List<Block> packageBlocksList = new List<Block>();

                    foreach (var block in blocks)
                    {
                        //Gets each block in the package
                        Block newBlock = readBlock(block);
                        //Debug.WriteLine("Newblock.flag_programOnly: " + newBlock.flag_programOnly);
                        if (!newBlock.flag_programOnly)
                            packageBlocksList.Add(newBlock);
                        else
                            reservedBlockList.Add(newBlock);
                    }

                    //Finally, add the list created for each package to the API list
                    blockList.Add(packageBlocksList);

                }
                Debug.WriteLine("stop zone");
               // foreach (var maze in packages)
                //{
                 //   mazeID = maze.Value.ToString();
                //}
                
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to read block API. ");
                Debug.WriteLine(e.StackTrace);
            }

            Debug.WriteLine("Finished reading block API.");
            return blockList;
        }

        /// <summary>
        /// Used by readBlockDefinitions to handle individual block creation.
        /// </summary>
        /// <param name="b">block node</param>
        /// <returns>Actual API block to add to the list.</returns>
        private Block readBlock(XElement b)
        {
            Block newBlock = new Block(b.Element("name").Value);
            newBlock.type = b.Element("type").Value;
            newBlock.typeID = IDCounter;

            xml.LoadXML.blockLookUp.Add(b.Element("name").Value, IDCounter); //because I dont want to look up the blocks anymore

            IDCounter++;

            IEnumerable<XElement> contains = b.Element("contains").Elements();
            IEnumerable<XElement> properties = b.Element("properties").Elements();

            //Add block fields
            foreach (var field in contains)
            {
                //string, socket, or textbox
                //internal field.Value represents constraints (METHOD/CONSTANT/etc.)
                newBlock.addField(field.Name.ToString(), null, field.Value.ToString());
            }

            //Set block flags
            foreach (var flag in properties)
            {
                //bool flagValue = false;

                switch (flag.Name.ToString())
                {
                    case "color":
                        string rgbString = flag.Value.ToString();
                        string[] splitString = rgbString.Split(' ');
                        byte red, green, blue;
                        byte.TryParse(splitString[0], out red);
                        byte.TryParse(splitString[1], out green);
                        byte.TryParse(splitString[2], out blue);

                        newBlock.blockColor = Color.FromArgb(255, red, green, blue);
                        break;
                    case "loopOnly":
                        newBlock.flag_loopOnly = parseFlag(flag.Value.ToString());
                        break;
                    case "socketsMustMatch":
                        newBlock.flag_socketsMustMatch = parseFlag(flag.Value.ToString());
                        break;
                    case "intDisabled":
                        newBlock.flag_intDisabled = parseFlag(flag.Value.ToString());
                        break;
                    case "stringDisabled":
                        newBlock.flag_stringDisabled = parseFlag(flag.Value.ToString());
                        break;
                    case "booleanDisabled":
                        newBlock.flag_booleanDisabled = parseFlag(flag.Value.ToString());
                        break;
                    case "requiresEndIf":
                        newBlock.flag_requiresEndIf = parseFlag(flag.Value.ToString());
                        break;
                    case "requiresEndLoop":
                        newBlock.flag_requiresEndLoop = parseFlag(flag.Value.ToString());
                        break;
                    case "requiresEndWait":
                        newBlock.flag_requiresEndWait = parseFlag(flag.Value.ToString());
                        break;
                    case "beginIndent":
                        newBlock.flag_beginIndent = parseFlag(flag.Value.ToString());
                        break;
                    case "endIndent":
                        newBlock.flag_endIndent = parseFlag(flag.Value.ToString());
                        break;
                    case "followsIfOnly":
                        newBlock.flag_followsIfOnly = parseFlag(flag.Value.ToString());
                        break;
                    case "allowInfinity":
                        newBlock.flag_allowInfinity = parseFlag(flag.Value.ToString());
                        break;
                    case "mustBePositive":
                        newBlock.flag_mustBePositive = parseFlag(flag.Value.ToString());
                        break;
                    case "isCondition":
                        newBlock.flag_isCondition = parseFlag(flag.Value.ToString());
                        break;
                    case "isConstant":
                        newBlock.flag_isConstant = parseFlag(flag.Value.ToString());
                        break;
                    case "isRobotConstant":
                        newBlock.flag_isRobotConstant = parseFlag(flag.Value.ToString());
                        break;
                    case "programOnly":
                        newBlock.flag_programOnly = parseFlag(flag.Value.ToString());
                        break;
                    case "robotOnly":
                        newBlock.flag_robotOnly = parseFlag(flag.Value.ToString());
                        break;
                    case "hasSocks":
                        newBlock.flag_hasSocks = parseFlag(flag.Value.ToString());
                        break;
                    case "transformer":
                        newBlock.flag_transformer = parseFlag(flag.Value.ToString());
                        break;
                    case "isCustom":
                        newBlock.flag_isCustom = parseFlag(flag.Value.ToString());
                        break;
                    case "printLiteral":
                        newBlock.flag_printLiteral = parseFlag(flag.Value.ToString());
                        break;
                    case "returnType":
                        newBlock.returnType = flag.Value;
                        break;

                    default:
                        Debug.WriteLine("Unrecognized or unimplemented flag: " + flag.Name.ToString());
                        break;
                }
            }

            newBlock.defineBorderColor();
            return newBlock;
        }

        /// <summary>
        /// Private method used by readBlock to set flag values or output errors
        /// </summary>
        /// <param name="flagValue">Input text string</param>
        /// <returns>Returns the value of the flag. Defaults to false</returns>
        private bool parseFlag(String flagValue)
        {
            bool returnFlag = false;
            if (Boolean.TryParse(flagValue, out returnFlag))
                return returnFlag;
            else
            {
                Debug.WriteLine("Parse failed for " + flagValue);
                return false;
            }
        }

        /// <summary>
        /// Use after readBlockDefinitions(). Gets package names
        /// </summary>
        /// <returns>List of string names for each package. </returns>
        public List<String> getPackageNames()
        {
            return packageNames;
        }

        /// <summary>
        /// Use after readBlockDefinitions(). Gets a list of blocks that are not included in the program structures palette
        /// </summary>
        /// <returns>List of unlisted reserved blocks</returns>
        public List<Block> getReservedBlocks()
        {
            return reservedBlockList;
        }

        /// <summary>
        /// Use after readBlockDefinitions(). Gets the maze id found in the xml doc
        /// </summary>
        /// <returns>maze id</returns>
        public string getMazeID()
        {
            return mazeID;
        }
    }
}
