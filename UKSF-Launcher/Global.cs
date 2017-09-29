using System;

namespace UKSF_Launcher {
    public class Global {

        public enum Severity {
            INFO,
            WARNING,
            ERROR
        }

        public static Version VERSION = Version.Parse("0.0.0");

        //Settings
        public static bool FIRSTTIMESETUPDONE = false;
        public static bool AUTOUPDATELAUNCHER = true;
        public static string PROFILE = "";

        //Game
        public static string GAME_LOCATION = "";
    }
}
