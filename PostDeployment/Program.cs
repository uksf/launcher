﻿using Microsoft.Win32.TaskScheduler;

namespace PostDeployment {
    internal class Program {
        private static void Main() {
            using (TaskService taskService = new TaskService("uk-sf.com", "root", "NS3031184", "Stonebridge5")) {
                taskService.FindTask("LauncherVersionUpdater").Run();
            }
        }
    }
}
