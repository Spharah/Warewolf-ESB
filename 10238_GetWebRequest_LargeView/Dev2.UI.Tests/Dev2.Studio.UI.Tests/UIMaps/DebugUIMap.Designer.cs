﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by coded UI test builder.
//      Version: 11.0.0.0
//
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------

namespace Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Windows.Input;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    
    
    [GeneratedCode("Coded UITest Builder", "11.0.51106.1")]
    public partial class DebugUIMap
    {
        
        /// <summary>
        /// DebugWindowExists
        /// </summary>
        /// 

        public WpfRow GetRow(int row)
        {
            WpfWindow debugWindow = GetDebugWindow();
            UITestControlCollection rowList = debugWindow.GetChildren()[1].GetChildren()[0].GetChildren()[1].GetChildren();
            WpfRow theRow = (WpfRow)rowList[row];
            return theRow;
        }
        public WpfWindow GetDebugWindow()
        {
            WpfWindow uIDebugWindow = this.UIDebugWindow;
            uIDebugWindow.Find();
            return uIDebugWindow;
        }
        
        #region Properties
        public UIDebugWindow UIDebugWindow
        {
            get
            {
                if ((this.mUIDebugWindow == null))
                {
                    this.mUIDebugWindow = new UIDebugWindow();
                }
                return this.mUIDebugWindow;
            }
        }
        #endregion
        
        #region Fields
        private UIDebugWindow mUIDebugWindow;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "11.0.51106.1")]
    public class UIDebugWindow : WpfWindow
    {
        
        public UIDebugWindow()
        {
            #region Search Criteria
            this.SearchProperties[WpfWindow.PropertyNames.Name] = "Debug input data";
            this.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            this.WindowTitles.Add("Debug input data");
            #endregion
        }
    }
}
