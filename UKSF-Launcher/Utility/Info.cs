using System;

namespace UKSF_Launcher.Utility {
    public class Info {

        public enum Severity {
            INFO,
            WARNING,
            ERROR
        }

        public static string LOGS = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/UKSF-Launcher";
        public static string FORMAT_DATE = "yyyy-MM-dd__HH-mm-ss";
        public static string FORMAT_TIME = "HH:mm:ss";
        public static string HASHSPACE = "\n#############################################";
    }
}
