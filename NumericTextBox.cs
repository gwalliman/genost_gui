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
using System.Globalization;

namespace CapGUI
{
    //As the name implies, handles the numeric text boxes
    public class NumericTextBox : TextBox
    {
        //rights to amurra ... http://stackoverflow.com/questions/268207/how-to-create-a-numeric-textbox-in-silverlight
        //When typing, we only want numbers to go into the text box
        //Handle all events (i.e. kill them from moving forward) if they are using anything but numbers
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Handle Shift case
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                e.Handled = true;
            }

            // Handle all other cases, OemMinus = keycode 189
            if (!e.Handled && (e.Key < Key.D0 || e.Key > Key.D9) && (e.Key != Key.Subtract) && (e.PlatformKeyCode != 189))
            {
                if (e.Key < Key.NumPad0 || e.Key > Key.NumPad9)
                {
                    if (e.Key != Key.Back)
                    {
                        e.Handled = true;
                    }
                }
            }
            base.OnKeyDown(e);
        }
    }
}
