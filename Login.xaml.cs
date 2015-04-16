using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CapGUI
{
    /**
     * Used to build the login box
     */
    public partial class Login : ChildWindow
    {
        private string name;
        private string password;
        private bool freeMode;

        public Login()
        {
            InitializeComponent();

            //Check if we have a user info cookie
            //If we do, use it to autofill
            string[] cookies = HtmlPage.Document.Cookies.Split(';');
            foreach (string cookie in cookies)
            {
                string cookieStr = cookie.Trim();
                if (cookieStr.StartsWith("username=", StringComparison.OrdinalIgnoreCase))
                {
                    string[] vals = cookieStr.Split('=');

                    if (vals.Length >= 2)
                    {
                        this.username.Text = vals[1];
                    }
                }
            }
        }

        //When clicking OK, run the TRUE handler
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        //When clicking Free Mode, run the FALSE handler
        private void FreeButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public bool getFreeMode()
        {
            return freeMode;
        }

        public string getUserName()
        {
            return name;
        }

        public string getPassword()
        {
            return password;
        }

        //Upon the login closing
        private void Login_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //We clicked OK
            if (this.DialogResult == true)
            {
                //Make sure both fields are filled
                if (this.username.Text == string.Empty || this.pass.Password == string.Empty)
                {
                    e.Cancel = true;
                    MessageBox.Show("Please Enter your name and password.", "Username and Password", MessageBoxButton.OK);
                }
                else
                {
                    //Make sure that both username and password are alphanumeric (sanitization)
                    Regex r = new Regex("^[a-zA-Z0-9]*$");
                    if (r.IsMatch(username.Text) && r.IsMatch(pass.Password))
                    {
                        name = username.Text;

                        //parses out symbols from password, since silverlight devs apparently don't want sql injection defences possible with their crappy passwordbox
                        //manages the situation until a better solution can be found
                        /**
                         * I think this is useless now
                         */
                        /*foreach (char c in pass.Password)
                        {
                            if (char.IsLetterOrDigit(c))
                            {
                                password += c;
                            }
                        }*/
                        password = pass.Password;

                        freeMode = false;
                        
                    }
                    else
                    {
                        e.Cancel = true;
                        MessageBox.Show("Please enter only alphanumeric characters!", "Alphanumeric Characters Only", MessageBoxButton.OK);
                    }
                }
            }
            //We clicked either CLOSE or FREE MODE
            else
            {
                //If we clicked Free Mode, log us in to Free Mode
                if (this.FreeButton.IsPressed)
                {
                    name = "freeModeUser";
                    password = "123456";
                    freeMode = true;
                    MessageBox.Show("Entering Free Mode...", "Entering Free Mode...", MessageBoxButton.OK);
                }
            }
        }
    }
}

