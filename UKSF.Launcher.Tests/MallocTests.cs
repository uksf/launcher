using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UKSF.Launcher.Game;

namespace UKSF.Launcher.Tests {
    internal class MallocTests {
        [Test]
        public void MallocTestsMallocCreation() {
            MallocHandler.Malloc malloc = new MallocHandler.Malloc(@"B:\Steam\steamapps\common\Arma 3\Dll\tbb4malloc_bi.dll");

            Assert.AreEqual(malloc.Name, "tbb4malloc_bi");
        }

        [Test]
        public void MallocTestsFind() {
            string installation = GameHandler.GetGameInstallation();
            if (string.IsNullOrEmpty(installation)) return;
            installation = Path.GetDirectoryName(installation);
            if (string.IsNullOrEmpty(installation)) return;
            List<MallocHandler.Malloc> mallocs = MallocHandler.GetMalloc(Path.Combine(installation, "Dll"));

            Assert.NotZero(mallocs.Count);
        }
    }
}
