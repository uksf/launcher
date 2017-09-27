using System;
using System.Collections.Generic;

namespace UKSF_Launcher {
    public class Global {

        public enum Severity {
            INFO,
            WARNING,
            ERROR
        }

        //Settings
        public static bool AUTOUPDATE = true;
        public static string PROFILE = "";

        //Game
        public static string GAME_LOCATION = "";
    }
}
