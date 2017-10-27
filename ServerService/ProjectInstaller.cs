using System.ComponentModel;
using System.Configuration.Install;

namespace ServerService {
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer {
        public ProjectInstaller() {
            InitializeComponent();
        }
    }
}