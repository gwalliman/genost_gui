using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CapGUI
{
    public class BlockDataList
    {
        private List<String> blockList;

        public BlockDataList()
        {
            this.blockList = new List<string>();
        }

        public BlockDataList(List<String> blockList)
        {
            this.blockList = new List<string>(blockList);
        }
    }
}
