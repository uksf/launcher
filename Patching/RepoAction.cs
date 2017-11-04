namespace Patching {
    internal class RepoAction {
        public RepoAction(Addon addon, string signatureFile) {
            Addon = addon;
            SignatureFile = signatureFile;
        }

        public Addon Addon { get; }
        public string SignatureFile { get; set; }

        public virtual void Consume() { }


        public class AddedAction : RepoAction {
            public AddedAction(Addon addon, string signatureFile) : base(addon, signatureFile) { }

            public override void Consume() { }
        }

        public class DeletedAction : RepoAction {
            public DeletedAction(Addon addon, string signatureFile) : base(addon, signatureFile) { }
        }

        public class ModifiedAction : RepoAction {
            public ModifiedAction(Addon addon, string signatureFile) : base(addon, signatureFile) { }
        }
    }
}