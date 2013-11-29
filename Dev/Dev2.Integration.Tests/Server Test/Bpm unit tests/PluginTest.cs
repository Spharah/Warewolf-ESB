﻿using System;
using Dev2.Common.ExtMethods;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests
{
    /// <summary>
    /// Summary description for PluginTest
    /// </summary>
    [TestClass]
    public class PluginTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("PluginIntegrationTest")]
        [Description("Test for executing a remote plugin, specific data is expected to be returned back")]
        [Owner("Ashley")]
        public void Plugins_PluginIntegrationTest_Execution_CorrectResponse()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "BUG_9966_RemotePlugins");
            const string expected = @"<DataList><Message>Exception of type 'HgCo.WindowsLive.SkyDrive.LogOnFailedException' was thrown.</Message></DataList>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(responseData.Unescape(), expected);
        }

        // This test is failing because the data does not bind to it ;)
        [TestMethod]
        public void TestPluginReturnsXMLFromXML()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningXMLfromXML");

            const string expected = @"<Names><CompanyName>Dev2</CompanyName><DepartmentName>Dev</DepartmentName><EmployeeName>Brendon</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Dev</DepartmentName><EmployeeName>Jayd</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Accounts</DepartmentName><EmployeeName>Bob</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Accounts</DepartmentName><EmployeeName>Joe</EmployeeName></Names>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(responseData.Unescape(), expected, " **** I expected { " + expected + " } but got { " + responseData + " }");
        }

        // Bug 8378
        [TestMethod]
        public void TestPluginsReturningXMLFromComplexType()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningXMLFromComplexType");
            const string expected = @"<Names><CompanyName>Dev2</CompanyName><DepartmentName>Dev</DepartmentName><EmployeeName>Brendon</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Dev</DepartmentName><EmployeeName>Jayd</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Accounts</DepartmentName><EmployeeName>Bob</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Accounts</DepartmentName><EmployeeName>Jo</EmployeeName>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue(responseData.IndexOf(expected, StringComparison.Ordinal) >= 0, "Got [ " + responseData + " ]");            
        }

        // Bug 8378
        [TestMethod]
        public void TestPluginsReturningXMLFromJson()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningXMLFromJson");
            string expected = @"<ScalarName>Dev2</ScalarName><Names><DepartmentName>Dev</DepartmentName><EmployeeName>Brendon</EmployeeName></Names><Names><DepartmentName>Dev</DepartmentName><EmployeeName>Jayd</EmployeeName></Names><Names><DepartmentName>Accounts</DepartmentName><EmployeeName>Bob</EmployeeName></Names><Names><DepartmentName>Accounts</DepartmentName><EmployeeName>Joe</EmployeeName></Names><OtherNames><Name>RandomData</Name></OtherNames><OtherNames><Name>RandomData1</Name></OtherNames>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            expected = TestHelper.CleanUp(expected);
            responseData = TestHelper.CleanUp(responseData);

            StringAssert.Contains(responseData, expected, " **** I expected { " + expected + " } but got { " + responseData + " }");
        }
    }
}