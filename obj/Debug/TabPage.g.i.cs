﻿#pragma checksum "C:\Users\Garret\Documents\My Dropbox\Master's Thesis\Code\genost_gui\TabPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "9C79301B6041E28339BD0119E3DB5B42"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using ButtonControlLibrary;
using CapGUI;
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
    
    
    public partial class TabPage : System.Windows.Controls.Page {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal CapGUI.EditorDragDropTarget tempDragDrop;
        
        internal System.Windows.Controls.ListBox tempListBox;
        
        internal System.Windows.Controls.ComboBox returnType;
        
        internal CapGUI.VariableStructureDragDropTarget parDragDrop;
        
        internal System.Windows.Controls.ListBox parameterBox;
        
        internal CapGUI.VariableStructureDragDropTarget returnDragDrop;
        
        internal System.Windows.Controls.ListBox returnBox;
        
        internal ButtonControlLibrary.MyButton addParamBtn;
        
        internal ButtonControlLibrary.MyButton deleteParamBtn;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/CapGUI;component/TabPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.tempDragDrop = ((CapGUI.EditorDragDropTarget)(this.FindName("tempDragDrop")));
            this.tempListBox = ((System.Windows.Controls.ListBox)(this.FindName("tempListBox")));
            this.returnType = ((System.Windows.Controls.ComboBox)(this.FindName("returnType")));
            this.parDragDrop = ((CapGUI.VariableStructureDragDropTarget)(this.FindName("parDragDrop")));
            this.parameterBox = ((System.Windows.Controls.ListBox)(this.FindName("parameterBox")));
            this.returnDragDrop = ((CapGUI.VariableStructureDragDropTarget)(this.FindName("returnDragDrop")));
            this.returnBox = ((System.Windows.Controls.ListBox)(this.FindName("returnBox")));
            this.addParamBtn = ((ButtonControlLibrary.MyButton)(this.FindName("addParamBtn")));
            this.deleteParamBtn = ((ButtonControlLibrary.MyButton)(this.FindName("deleteParamBtn")));
        }
    }
}

