using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nDumbster.Smtp;

namespace nDumbster.MsTests
{
    [TestClass]
    public class SmtpSetup
    {
        public static SimpleSmtpServer SmtpServer { get; set; }

        [AssemblyInitialize]
        public static void Setup(TestContext testContext)
        {
            SmtpServer = SimpleSmtpServer.Start(25);
        }

        [AssemblyCleanup]
        public static void TearDown()
        {
            SmtpServer.Stop();
        }
    }
}
