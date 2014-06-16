﻿#pragma checksum "C:\Users\Tracey\Documents\GitHub\genost_gui\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "4FE0EACA969291467398C152AF91B6A7"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using ButtonControlLibrary;
using CapGUI;
using ExtendedTabControl;
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
    
    
    public partial class MainPage : System.Windows.Controls.UserControl {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid roboGrid;
        
        internal ButtonControlLibrary.MyButton roboLoadBtn;
        
        internal System.Windows.Controls.ListBox robotPalette;
        
        internal System.Windows.Controls.Grid blockGrid;
        
        internal System.Windows.Controls.ListBox blockPalette;
        
        internal System.Windows.Controls.Grid variableGrid;
        
        internal ButtonControlLibrary.MyButton createVariableBtn;
        
        internal ButtonControlLibrary.MyButton deleteVariableBtn;
        
        internal System.Windows.Controls.ListBox variablePalette;
        
        internal System.Windows.Controls.Grid methodGrid;
        
        internal ButtonControlLibrary.MyButton createMethodBtn;
        
        internal ButtonControlLibrary.MyButton deleteMethodBtn;
        
        internal System.Windows.Controls.ListBox methodPalette;
        
        internal System.Windows.Controls.Grid editorPanelGrid;
        
        internal System.Windows.Controls.StackPanel editorPanel;
        
        internal ExtendedTabControl.ExtendedTabControl editorTabControl;
        
        internal System.Windows.Controls.TabItem editorMain;
        
        internal CapGUI.EditorDragDropTarget editorDragDrop;
        
        internal System.Windows.Controls.ListBox editorPalette;
        
        internal ButtonControlLibrary.MyButton exCodeBtn;
        
        internal System.Windows.Controls.TextBlock btnLabel;
        
        internal ButtonControlLibrary.MyButton sendCodeBtn;
        
        internal System.Windows.Shapes.Ellipse codeStatusEllipse;
        
        internal ButtonControlLibrary.MyButton loadProgramBtn;
        
        internal ButtonControlLibrary.MyButton saveBtn;
        
        internal ButtonControlLibrary.MyButton runSimBtn;
        
        internal System.Windows.Media.Animation.Storyboard myStoryboard;
        
        internal System.Windows.Shapes.Ellipse MyAnimatedEllipse;
        
        internal System.Windows.Controls.Grid trashGrid;
        
        internal System.Windows.Controls.TextBlock errorMsg;
        
        internal System.Windows.Controls.Image trashImage;
        
        internal ButtonControlLibrary.MyButton clearBtn;
        
        internal CapGUI.TrashDragDropTarget trashDragDrop;
        
        internal System.Windows.Controls.ListBox trash;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/CapGUI;component/MainPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.roboGrid = ((System.Windows.Controls.Grid)(this.FindName("roboGrid")));
            this.roboLoadBtn = ((ButtonControlLibrary.MyButton)(this.FindName("roboLoadBtn")));
            this.robotPalette = ((System.Windows.Controls.ListBox)(this.FindName("robotPalette")));
            this.blockGrid = ((System.Windows.Controls.Grid)(this.FindName("blockGrid")));
            this.blockPalette = ((System.Windows.Controls.ListBox)(this.FindName("blockPalette")));
            this.variableGrid = ((System.Windows.Controls.Grid)(this.FindName("variableGrid")));
            this.createVariableBtn = ((ButtonControlLibrary.MyButton)(this.FindName("createVariableBtn")));
            this.deleteVariableBtn = ((ButtonControlLibrary.MyButton)(this.FindName("deleteVariableBtn")));
            this.variablePalette = ((System.Windows.Controls.ListBox)(this.FindName("variablePalette")));
            this.methodGrid = ((System.Windows.Controls.Grid)(this.FindName("methodGrid")));
            this.createMethodBtn = ((ButtonControlLibrary.MyButton)(this.FindName("createMethodBtn")));
            this.deleteMethodBtn = ((ButtonControlLibrary.MyButton)(this.FindName("deleteMethodBtn")));
            this.methodPalette = ((System.Windows.Controls.ListBox)(this.FindName("methodPalette")));
            this.editorPanelGrid = ((System.Windows.Controls.Grid)(this.FindName("editorPanelGrid")));
            this.editorPanel = ((System.Windows.Controls.StackPanel)(this.FindName("editorPanel")));
            this.editorTabControl = ((ExtendedTabControl.ExtendedTabControl)(this.FindName("editorTabControl")));
            this.editorMain = ((System.Windows.Controls.TabItem)(this.FindName("editorMain")));
            this.editorDragDrop = ((CapGUI.EditorDragDropTarget)(this.FindName("editorDragDrop")));
            this.editorPalette = ((System.Windows.Controls.ListBox)(this.FindName("editorPalette")));
            this.exCodeBtn = ((ButtonControlLibrary.MyButton)(this.FindName("exCodeBtn")));
            this.btnLabel = ((System.Windows.Controls.TextBlock)(this.FindName("btnLabel")));
            this.sendCodeBtn = ((ButtonControlLibrary.MyButton)(this.FindName("sendCodeBtn")));
            this.codeStatusEllipse = ((System.Windows.Shapes.Ellipse)(this.FindName("codeStatusEllipse")));
            this.loadProgramBtn = ((ButtonControlLibrary.MyButton)(this.FindName("loadProgramBtn")));
            this.saveBtn = ((ButtonControlLibrary.MyButton)(this.FindName("saveBtn")));
            this.runSimBtn = ((ButtonControlLibrary.MyButton)(this.FindName("runSimBtn")));
            this.myStoryboard = ((System.Windows.Media.Animation.Storyboard)(this.FindName("myStoryboard")));
            this.MyAnimatedEllipse = ((System.Windows.Shapes.Ellipse)(this.FindName("MyAnimatedEllipse")));
            this.trashGrid = ((System.Windows.Controls.Grid)(this.FindName("trashGrid")));
            this.errorMsg = ((System.Windows.Controls.TextBlock)(this.FindName("errorMsg")));
            this.trashImage = ((System.Windows.Controls.Image)(this.FindName("trashImage")));
            this.clearBtn = ((ButtonControlLibrary.MyButton)(this.FindName("clearBtn")));
            this.trashDragDrop = ((CapGUI.TrashDragDropTarget)(this.FindName("trashDragDrop")));
            this.trash = ((System.Windows.Controls.ListBox)(this.FindName("trash")));
        }
    }
}

