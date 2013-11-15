﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by coded UI test builder.
//      Version: 11.0.0.0
//
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------

using System.Text;
using System.Threading;
using System.Windows.Forms;
using Dev2.CodedUI.Tests;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;

namespace Dev2.Studio.UI.Tests.UIMaps.WebServiceWizardUIMapClasses
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

    public partial class WebServiceWizardUIMap : UIMapBase
    {
        public void InitializeFullTestServiceAndSource(string serviceName, string sourceName)
        {
            //init
            var explorer = new ExplorerUIMap();

            //Open Web Source Wizard
            explorer.ClearExplorerSearchText();
            explorer.EnterExplorerSearchText("$");
            var getLocalServer = explorer.GetLocalServer();
            var menuPt = new Point(getLocalServer.BoundingRectangle.X, getLocalServer.BoundingRectangle.Y);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, menuPt);

            Playback.Wait(300);

            menuPt.Offset(5, 5);
            Mouse.Move(menuPt);
            Playback.Wait(300);

            for (var i = 0; i < 9; i++)
            {
                Keyboard.SendKeys("{DOWN}");
            }
            SendKeys.SendWait("{ENTER}");

            //Wait for wizard
            WizardsUIMap.WaitForWizard();
            Playback.Wait(100);

            //Web Source Details
            SendKeys.SendWait("{TAB}http://www.webservicex.net/globalweather.asmx{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(100);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(100);
            SendKeys.SendWait("{TAB}{TAB}{TAB}" + sourceName + "{TAB}{ENTER}");

            //Open Web Service Wizard
            getLocalServer = explorer.GetLocalServer();
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, new Point(getLocalServer.BoundingRectangle.X, getLocalServer.BoundingRectangle.Y));
            for (var i = 0; i < 5; i++)
            {
                Playback.Wait(200);
                SendKeys.SendWait("{DOWN}");
            }

            Playback.Wait(500);
            SendKeys.SendWait("{ENTER}");

            //Wait for wizard
            WizardsUIMap.WaitForWizard();

            //Web Service Details
            SendKeys.SendWait("{TAB}{TAB}{DOWN}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(5000);//wait for test
            SendKeys.SendWait("{TAB}{ENTER}");
            Playback.Wait(2000);
            SendKeys.SendWait("{TAB}{TAB}{TAB}" + serviceName + "{TAB}{ENTER}");
            
        }

        public static void Cancel()
        {
            for (var i = 0; i < 4; i++)
            {
                Keyboard.SendKeys("{TAB}");
            }
            Keyboard.SendKeys("{ENTER}");
        }
    }

    [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
    public class UIStartPageCustom : WpfCustom
    {
        
        public UIStartPageCustom(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[UITestControl.PropertyNames.ClassName] = "Uia.ContentPane";
            this.SearchProperties["AutomationId"] = "splurt";
            this.WindowTitles.Add(TestBase.GetStudioWindowName());
            #endregion
        }
        
        #region Properties
        public WpfImage UIItemImage
        {
            get
            {
                if ((this.mUIItemImage == null))
                {
                    this.mUIItemImage = new WpfImage(this);
                    #region Search Criteria
                    this.mUIItemImage.WindowTitles.Add(TestBase.GetStudioWindowName());
                    #endregion
                }
                return this.mUIItemImage;
            }
        }
        #endregion
        
        #region Fields
        private WpfImage mUIItemImage;
        #endregion
    }
}
