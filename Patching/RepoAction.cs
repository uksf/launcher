namespace Patching {
    internal class RepoAction {
        public RepoAction(Addon addon, string addonFile) {
            Addon = addon;
            AddonFile = addonFile;
        }

        public Addon Addon { get; }
        public string AddonFile { get; set; }


        public class AddedAction : RepoAction {
            public AddedAction(Addon addon, string addonFile) : base(addon, addonFile) { }
        }

        public class DeletedAction : RepoAction {
            public DeletedAction(Addon addon, string addonFile) : base(addon, addonFile) { }
        }

        public class ModifiedAction : RepoAction {
            public ModifiedAction(Addon addon, string addonFile) : base(addon, addonFile) { }
        }
    }
}