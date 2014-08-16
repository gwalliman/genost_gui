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

namespace ButtonControlLibrary
{
    public class MyButton : Button
    {
        public MyButton()
        {
            DefaultStyleKey = typeof(MyButton);
        }
    }
    public class MainButton : Button
    {
        public MainButton()
        {
            DefaultStyleKey = typeof(MainButton);
        }
    }
}
