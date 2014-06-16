using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CapGUI
{

    public partial class Login : ChildWindow
    {
        private string name;
        private string password;

        public Login()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Login_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //checking if both fields are filled
            if (this.DialogResult == true && (this.username.Text == string.Empty || this.pass.Password == string.Empty))
            {
                e.Cancel = true;
                ChildWindow cw = new ChildWindow();
                cw.Content = "Please Enter your name and password.";
                cw.Show();
            }
            else
            {
                name = username.Text;

                //parses out symbols from password, since silverlight devs apparently don't want sql injection defences possible with their crappy passwordbox
                //manages the situation until a better solution can be found
                foreach (char c in pass.Password)
                {
                    if (char.IsLetterOrDigit(c))
                    {
                        password += c;
                    }
                }
            }

        }
    }

    //class that limits textbox input to only alphanumeric characters
    public partial class credBox : TextBox
    {

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Handle Shift case
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                e.Handled = true;
            }

            if(char.IsLetterOrDigit((char)e.Key))
            {
                e.Handled = true;

            }
            else
            {
                e.Handled = false;

            }
        }
    }
   
}

