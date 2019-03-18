using System.ComponentModel;
using System.Configuration.Install;

namespace UKSF.Launcher.ServerService {
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer {
        public ProjectInstaller() {
            InitializeComponent();
        }
    }
}