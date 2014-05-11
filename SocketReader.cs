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
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;

namespace CapGUI.Parsing
{
    public class SocketReader
    {
        public SocketReader()
        {
        }

        public Block readSocket(Block source, Block destination)
        {
            StackPanel innards = (StackPanel)source.innerPane.Children.ElementAt(0);
            List<System.Windows.UIElement> components = innards.Children.ToList();
            Debug.WriteLine("+" + components.ElementAt(0));
            destination.innerPane.Children.Clear();
            destination.innerPane.Children.Insert(0, source.innerPane.Children.ElementAt(0));
            return destination;
        }
    }
}
