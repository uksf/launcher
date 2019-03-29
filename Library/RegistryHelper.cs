using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Library {
    public class RegistryHelper {
        public async Task<object> GetArmaInstallLocation(object input) {
            return await Task.Run(() => {
                RegistryKey gameKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\bohemia interactive\arma 3");
                if (gameKey == null) return "";
                string install = gameKey.GetValue("main", "").ToString();
                return Directory.Exists(install) ? install : "";
            });
        }
    }
}
