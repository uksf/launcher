using NUnit.Framework;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.Tests {
    internal class ServerTests {
        [Test]
        public void ServerCreation() {
            ServerHandler.Server server = new ServerHandler.Server {Active = false, Ip = "uk-sf.com", Name = "Primary Server", Password = "l85", Port = 2303};

            Assert.AreEqual(server.Ip, "uk-sf.com");
            Assert.AreEqual(server.Name, "Primary Server");
            Assert.AreEqual(server.Password, "l85");
            Assert.AreEqual(server.Port, 2303);
        }
    }
}