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

    public partial class Login : ChildWindow
    {
        private string name;
        private string password;
        private bool freeMode;

        public Login()
        {
            InitializeComponent();
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

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

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

        private void Login_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //checking if both fields are filled
            if (this.DialogResult == true)
            {
                if (this.username.Text == string.Empty || this.pass.Password == string.Empty)
                {
                    e.Cancel = true;
                    ChildWindow cw = new ChildWindow();
                    cw.Content = "Please Enter your name and password.";
                    cw.Show();
                }
                else
                {
                    Regex r = new Regex("^[a-zA-Z0-9]*$");
                    if (r.IsMatch(username.Text) && r.IsMatch(pass.Password))
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

                        freeMode = false;
                        
                    }
                    else
                    {
                        e.Cancel = true;
                        ChildWindow cw = new ChildWindow();
                        cw.Content = "Please enter only alphanumeric characters!";
                        cw.Show();
                    }
                }
            }
            else
            {
                name = "freeModeUser";
                password = "123456";
                freeMode = true;
                ChildWindow cw = new ChildWindow();
                cw.Content = "Entering Free Mode...";
                cw.Show();
            }
        }
    }
}

