﻿#pragma checksum "C:\Users\David\Documents\GitHub\genost_gui\Block.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "77AD3DFB9F737855D7802AC1218A00A0"
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
    
    
    public partial class Block : System.Windows.Controls.UserControl {
        
        internal System.Windows.Controls.Border blockBorder;
        
        internal System.Windows.Controls.Canvas LayoutRoot;
        
        internal System.Windows.Controls.Grid blockGrid;
        
        internal System.Windows.Controls.StackPanel innerPane;
        
        internal System.Windows.Controls.TextBlock line;
        
        internal System.Windows.Controls.TextBlock fore;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/CapGUI;component/Block.xaml", System.UriKind.Relative));
            this.blockBorder = ((System.Windows.Controls.Border)(this.FindName("blockBorder")));
            this.LayoutRoot = ((System.Windows.Controls.Canvas)(this.FindName("LayoutRoot")));
            this.blockGrid = ((System.Windows.Controls.Grid)(this.FindName("blockGrid")));
            this.innerPane = ((System.Windows.Controls.StackPanel)(this.FindName("innerPane")));
            this.line = ((System.Windows.Controls.TextBlock)(this.FindName("line")));
            this.fore = ((System.Windows.Controls.TextBlock)(this.FindName("fore")));
        }
    }
}

