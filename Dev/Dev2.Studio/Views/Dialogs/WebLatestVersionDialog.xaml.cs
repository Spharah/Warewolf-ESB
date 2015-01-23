
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows.Navigation;
using Dev2.Common.Interfaces;

namespace Dev2.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for WebLatestVersionDialog.xaml
    /// </summary>
    public partial class WebLatestVersionDialog
    {
        public WebLatestVersionDialog()
        {
            InitializeComponent();
            Browser.Navigate(new Uri("http://www.warewolf.io/start_new.php"));
        }

        void wb_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Browser.Width = Browser.ActualWidth + 32; 
            Browser.Height = Browser.ActualHeight + 32;
        }

        #region Implementation of IWebLatestVersionDialog

       

        #endregion
    }
}
