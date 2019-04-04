using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Library {
    public class FileHelper {
        public async Task<object> GetDriveSpace(object input) => await Task.Run(() => DriveInfo.GetDrives().Where(drive => drive.Name.Equals(Path.GetPathRoot((string) input))).ElementAt(0).AvailableFreeSpace);
    }
}
