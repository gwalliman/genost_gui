﻿#pragma checksum "C:\Users\Tracey\Documents\GitHub\genost_gui\PopupInterface.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "0E6C1975173ECE98B67CDD2FA170FFDF"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace CapGUI {
    
    
    public partial class PopupInterface : System.Windows.Controls.UserControl {
        
        internal System.Windows.Controls.Primitives.Popup MenuPopup;
        
        internal System.Windows.Controls.Canvas MenuCanvas;
        
        internal System.Windows.Controls.TextBox PopupTextBox;
        
        internal System.Windows.Controls.TextBlock PopupDeleteInfo;
        
        internal System.Windows.Controls.TextBlock DeleteConfirm;
        
        internal System.Windows.Controls.TextBlock type;
        
        internal System.Windows.Controls.ComboBox PopupComboBox;
        
        internal System.Windows.Controls.Button OkAddBtn;
        
        internal System.Windows.Controls.Button OkEditBtn;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/CapGUI;component/PopupInterface.xaml", System.UriKind.Relative));
            this.MenuPopup = ((System.Windows.Controls.Primitives.Popup)(this.FindName("MenuPopup")));
            this.MenuCanvas = ((System.Windows.Controls.Canvas)(this.FindName("MenuCanvas")));
            this.PopupTextBox = ((System.Windows.Controls.TextBox)(this.FindName("PopupTextBox")));
            this.PopupDeleteInfo = ((System.Windows.Controls.TextBlock)(this.FindName("PopupDeleteInfo")));
            this.DeleteConfirm = ((System.Windows.Controls.TextBlock)(this.FindName("DeleteConfirm")));
            this.type = ((System.Windows.Controls.TextBlock)(this.FindName("type")));
            this.PopupComboBox = ((System.Windows.Controls.ComboBox)(this.FindName("PopupComboBox")));
            this.OkAddBtn = ((System.Windows.Controls.Button)(this.FindName("OkAddBtn")));
            this.OkEditBtn = ((System.Windows.Controls.Button)(this.FindName("OkEditBtn")));
        }
    }
}

