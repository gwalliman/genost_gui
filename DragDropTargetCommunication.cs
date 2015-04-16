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
    /**
     * This class seems to be a way to track the global state when something is being dragged
     **/
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

        //Check to see if the color status of set code is orange
        //If not then change status color to alter to new code change
        public void changeCodeColorStatus()
        {
            if(!((SolidColorBrush)MainPage.Instance.codeStatusEllipse.Fill).Color.Equals(Colors.Black))
                if (!((SolidColorBrush)MainPage.Instance.codeStatusEllipse.Fill).Color.Equals(Colors.Orange))
                    MainPage.Instance.setStatusEllipse("orange");
        }
    }
}
