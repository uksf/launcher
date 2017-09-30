using UKSF_Launcher.UI.FTS;
using static UKSF_Launcher.Global;

namespace UKSF_Launcher.Utility {
    public class SetupHandler {
        public static void FirstTimeSetup() {
            LogHandler.LogHashSpaceMessage(Severity.INFO, "Running first time setup");

            new FtsWindow().ShowDialog();
        }
    }
}