﻿#pragma checksum "..\..\..\MainWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "B839E657A482EE4647C7C7927249879A20E852F74AC822B6488D75C1EA24D399"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using HamburgerMenu;
using PRL123_Final;
using PRL123_Final.ViewModels;
using PRL123_Final.Views;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace PRL123_Final {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 83 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock CurrentVersion;
        
        #line default
        #line hidden
        
        
        #line 84 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock NewVersion;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/PRL123_Final;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 11 "..\..\..\MainWindow.xaml"
            ((PRL123_Final.MainWindow)(target)).Loaded += new System.Windows.RoutedEventHandler(this.MainWindow_OnLoaded);
            
            #line default
            #line hidden
            
            #line 11 "..\..\..\MainWindow.xaml"
            ((PRL123_Final.MainWindow)(target)).ContentRendered += new System.EventHandler(this.contentRendered);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 69 "..\..\..\MainWindow.xaml"
            ((HamburgerMenu.HamburgerMenuItem)(target)).Selected += new System.Windows.RoutedEventHandler(this.Eaton_Clicked);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 70 "..\..\..\MainWindow.xaml"
            ((HamburgerMenu.HamburgerMenuItem)(target)).Selected += new System.Windows.RoutedEventHandler(this.Specialist_Clicked);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 71 "..\..\..\MainWindow.xaml"
            ((HamburgerMenu.HamburgerMenuItem)(target)).Selected += new System.Windows.RoutedEventHandler(this.Supervisor_Clicked);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 72 "..\..\..\MainWindow.xaml"
            ((HamburgerMenu.HamburgerMenuItem)(target)).Selected += new System.Windows.RoutedEventHandler(this.materialPlanning_Clicked);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 73 "..\..\..\MainWindow.xaml"
            ((HamburgerMenu.HamburgerMenuItem)(target)).Selected += new System.Windows.RoutedEventHandler(this.Assembler_Clicked);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 74 "..\..\..\MainWindow.xaml"
            ((HamburgerMenu.HamburgerMenuItem)(target)).Selected += new System.Windows.RoutedEventHandler(this.Shipping_Clicked);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 75 "..\..\..\MainWindow.xaml"
            ((HamburgerMenu.HamburgerMenuItem)(target)).Selected += new System.Windows.RoutedEventHandler(this.Analytics_Clicked);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 76 "..\..\..\MainWindow.xaml"
            ((HamburgerMenu.HamburgerMenuItem)(target)).Selected += new System.Windows.RoutedEventHandler(this.Settings_Clicked);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 77 "..\..\..\MainWindow.xaml"
            ((HamburgerMenu.HamburgerMenuItem)(target)).Selected += new System.Windows.RoutedEventHandler(this.Configuration_Clicked);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 78 "..\..\..\MainWindow.xaml"
            ((HamburgerMenu.HamburgerMenuItem)(target)).Selected += new System.Windows.RoutedEventHandler(this.Credits_Clicked);
            
            #line default
            #line hidden
            return;
            case 12:
            this.CurrentVersion = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 13:
            this.NewVersion = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

