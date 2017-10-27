using Network;
using NUnit.Framework;

namespace UKSF_Launcher.Tests {
    internal class ServerTests {
        [Test]
        public void ServerTestsServerCreation() {
            Server server = new Server("Primary Server", "uk-sf.com", 2303, "l85", true);

            Assert.AreEqual(server.Ip, "uk-sf.com");
            Assert.AreEqual(server.Name, "Primary Server");
            Assert.AreEqual(server.Password, "l85");
            Assert.AreEqual(server.Port, 2303);
            Assert.IsTrue(server.Active);
        }
    }
}