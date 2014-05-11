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

namespace CapGUI
{
    public class DragDropTargetCommunication
    {
        public bool trash { get; set; }
        public bool socket { get; set; }
        public bool editor { get; set; }

        public DragDropTargetCommunication()
        {
            trash = false;
            socket = false;
            editor = false;
        }
    }
}
