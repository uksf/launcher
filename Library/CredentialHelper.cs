using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 1998

namespace Library {
    public class CredentialHelper {
        private static readonly byte[] ENTROPY = {2, 25, 12, 76, 21, 7};

        public async Task<object> Decrypt(object input) {
            return await Task.Run(() => {
                byte[] encryptedText = Convert.FromBase64String((string) input);
                byte[] output = ProtectedData.Unprotect(encryptedText, ENTROPY, DataProtectionScope.CurrentUser);
                return Encoding.Unicode.GetString(output);
            });
        }

        public async Task<object> Encrypt(object input) {
            return await Task.Run(() => {
                byte[] originalText = Encoding.Unicode.GetBytes((string) input);
                byte[] output = ProtectedData.Protect(originalText, ENTROPY, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(output);
            });
        }
    }
}
