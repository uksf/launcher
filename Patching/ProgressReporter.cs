using System;

namespace Patching {
    public class ProgressReporter {
        protected ProgressReporter(Action<string> progressAction) => ProgressAction = progressAction;
        protected readonly Action<string> ProgressAction;
    }
}