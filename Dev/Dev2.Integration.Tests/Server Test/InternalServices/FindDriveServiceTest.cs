﻿using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices {
    /// <summary>
    /// Summary description for FindDriveServiceTest
    /// </summary>
    [TestClass]
    [Ignore] // StringBuilder refactor
    public class FindDriveServiceTest {

        private readonly string WebserverUrl = TestResource.WebserverURI_Local;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void FindDriveService_NoParameters() {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDriveService");
            string expected = @"[{""driveLetter"":""C:/""";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            Assert.IsTrue(responseData.Contains(expected));
        }

        [TestMethod]
        public void FindDriveService_ValidCredentials()
        {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDriveService?Domain=DEV2&Username=" + TestResource.PathOperations_Correct_Username + "&Password=" + TestResource.PathOperations_Correct_Password);
            string expected = @"[{""driveLetter"":""C:/""";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            Assert.IsTrue(responseData.Contains(expected));
        }

        [TestMethod]
        public void FindDriveService_InvalidCredentials() {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDriveService?Domain=DEV2&Username=john.doe&Password=P@ssword");
            string expected = @"<result>Logon failure: unknown user name or bad password</result>";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            StringAssert.Contains(responseData, expected);
        }
    }
}
