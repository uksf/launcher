using Microsoft.Win32.TaskScheduler;

namespace PostDeployment {
    class Program {
        static void Main(string[] args) {
            using (TaskService taskService = new TaskService("uk-sf.com", "root", "NS3031184", "Stonebridge5")) {
                taskService.FindTask("LauncherVersionUpdater").Run();
            }
        }
    }
}
