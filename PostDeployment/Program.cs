using Microsoft.Win32.TaskScheduler;

namespace PostDeployment {
    class Program {
        static void Main(string[] args) {
            using (TaskService tasksrvc = new TaskService("uk-sf.com", "root", "NS3031184", "Stonebridge5")) {
                Task task = tasksrvc.FindTask("LauncherVersionUpdater");
                task.Run();
            }
        }
    }
}
