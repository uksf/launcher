using System.Collections.Generic;
using System.Windows;
using Network;
using UKSF_Launcher.UI.Main;

namespace UKSF_Launcher.UI.General {
    public class SafeWindow : Window {
        /// <inheritdoc />
        /// <summary>
        ///     Creates new Window if current Application still exists, otherwise forces program exit.
        /// </summary>
        protected SafeWindow() {
            if (Application.Current == null) {
                Core.ShutDown();
            }
        }

        public class StringRoutedEventArgs : RoutedEventArgs {
            public StringRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public string Text { get; set; }
        }

        public class BoolRoutedEventArgs : RoutedEventArgs {
            public BoolRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public bool State { get; set; }
        }

        public class IntRoutedEventArgs : RoutedEventArgs {
            public IntRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public int Value { get; set; }
        }

        public class ProgressRoutedEventArgs : RoutedEventArgs {
            public ProgressRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public int Value { get; set; }
            public string Message { get; set; }
        }

        public class WarningRoutedEventArgs : RoutedEventArgs {
            public WarningRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public bool Block { get; set; }
            public string Warning { get; set; }
            public MainMainControl.CurrentWarning CurrentWarning { get; set; }
        }

        public class ServerRoutedEventArgs : RoutedEventArgs {
            public ServerRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public List<Server> Servers { get; set; }
        }
    }
}