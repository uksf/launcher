using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.Tests {
    internal class MallocTests {
        [Test]
        public void MallocCreation() {
            MallocHandler.Malloc malloc = new MallocHandler.Malloc(@"B:\Steam\steamapps\common\Arma 3\Dll\tbb4malloc_bi.dll");

            Assert.AreEqual(malloc.Name, "tbb4malloc_bi");
        }

        [Test]
        public void MallocFind() {
            string installation = GameHandler.GetGameInstallation();
            if (string.IsNullOrEmpty(installation)) return;
            installation = Path.GetDirectoryName(installation);
            if (string.IsNullOrEmpty(installation)) return;
            List<MallocHandler.Malloc> mallocs = MallocHandler.GetMalloc(Path.Combine(installation, "Dll"));

            Assert.NotZero(mallocs.Count);
        }
    }
}