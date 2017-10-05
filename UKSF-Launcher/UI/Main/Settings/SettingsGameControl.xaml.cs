using System.Collections.Generic;
using System.IO;
using System.Windows;
using UKSF_Launcher.Game;
using UKSF_Launcher.UI.General;
using static UKSF_Launcher.Global;

namespace UKSF_Launcher.UI.Main.Settings {
    /// <summary>
    ///     Interaction logic for SettingsGameControl.xaml
    /// </summary>
    public partial class SettingsGameControl {
        public static readonly RoutedEvent SETTINGS_GAME_CONTROL_WARNING_EVENT =
            EventManager.RegisterRoutedEvent("SETTINGS_GAME_CONTROL_WARNING_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(SettingsGameControl));

        /// <inheritdoc />
        /// <summary>
        ///     Creates new MainMainControl object.
        /// </summary>
        public SettingsGameControl() {
            InitializeComponent();
        }

        public void Initialise() {
            AddHandler(ProfileSelectionControl.PROFILE_SELECTION_CONTROL_UPDATE_EVENT, new RoutedEventHandler(SettingsLauncherControlProfile_Update));
            AddHandler(SETTINGS_GAME_CONTROL_WARNING_EVENT, new RoutedEventHandler(SettingsLauncherControlProfile_Update));

            SettingsGameControlCheckboxSplash.IsChecked = STARTUP_NOSPLASH;
            SettingsGameControlCheckboxWorld.IsChecked = STARTUP_EMPTYWORLD;
            SettingsGameControlCheckboxScript.IsChecked = STARTUP_SCRIPTERRORS;
            SettingsGameControlCheckboxPages.IsChecked = STARTUP_HUGEPAGES;
            SettingsGameControlCheckboxShacktac.IsChecked = MODS_SHACKTAC;

            SettingsGameControlDevControl.IsEnabled = false;
            SettingsGameControlCheckboxPatching.IsChecked = false;
#if DEV
            SettingsGameControlDevControl.IsEnabled = true;
            SettingsGameControlCheckboxPatching.IsChecked = STARTUP_FILEPATCHING;
#endif

            if (SettingsGameControlDropdownMalloc.Items.IsEmpty) {
                AddMallocs();
            }

            UpdateWarning();
        }

        /// <summary>
        ///     Adds mallocs to malloc dropdown. Sets selected malloc to STARTP_MALLOC value if it is set.
        /// </summary>
        private void AddMallocs() {
            SettingsGameControlDropdownMalloc.Items.Clear();
            SettingsGameControlDropdownMalloc.Items.Add(new MallocComboBoxItem(MALLOC_SYSTEM_DEFAULT, FindResource("Uksf.ComboBoxItem") as Style));
            List<MallocHandler.Malloc> mallocs = MallocHandler.GetMalloc(Path.Combine(GAME_LOCATION, "..", "Dll"));
            foreach (MallocHandler.Malloc malloc in mallocs) {
                SettingsGameControlDropdownMalloc.Items.Add(new MallocComboBoxItem(malloc, FindResource("Uksf.ComboBoxItem") as Style));

                if (malloc.Name == STARTUP_MALLOC) {
                    SettingsGameControlDropdownMalloc.SelectedIndex = mallocs.IndexOf(malloc);
                }
            }

            if (SettingsGameControlDropdownMalloc.SelectedIndex > -1) return;
            SettingsGameControlDropdownMalloc.SelectedIndex = 0;
        }

        /// <summary>
        ///     Triggered by eventhandler. Writes profile to settings registry.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Selected arguments</param>
        private void SettingsLauncherControlProfile_Update(object sender, RoutedEventArgs args) {
            if (SettingsGameControlDropdownProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedItem != null) {
                if (PROFILE != ((ProfileComboBoxItem) SettingsGameControlDropdownProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedItem).Profile.DisplayName) {
                    PROFILE = (string) Core.SettingsHandler.WriteSetting("PROFILE",
                                                                         ((ProfileComboBoxItem) SettingsGameControlDropdownProfileSelectionControl
                                                                             .ProfileSelectionControlDropdownProfile.SelectedItem).Profile.DisplayName);
                }
            }
            UpdateWarning();
        }

        private void UpdateWarning() {
            string warning = "";
            bool block = false;
            if (SettingsGameControlDropdownProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedIndex == -1) {
                warning = "Please select a profile";
                block = true;
            }
            SettingsGameControlProfileWarningText.Content = warning;
            if (MainWindow.Instance.MainMainControl == null) return;
            MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.WarningRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_WARNING_EVENT) {
                Warning = warning,
                Block = block,
                CurrentWarning = MainMainControl.CurrentWarning.PROFILE
            });
        }

        /// <summary>
        ///     Triggered when malloc selection is changed. Updates malloc value.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Selected arguments</param>
        private void SettingsLauncherControlMalloc_Update(object sender, RoutedEventArgs args) {
            if (SettingsGameControlDropdownMalloc.SelectedIndex == -1 || STARTUP_MALLOC == ((MallocComboBoxItem) SettingsGameControlDropdownMalloc.SelectedItem).Malloc.Name) {
                return;
            }
            STARTUP_MALLOC = (string) Core.SettingsHandler.WriteSetting("STARTUP_MALLOC", ((MallocComboBoxItem) SettingsGameControlDropdownMalloc.SelectedItem).Malloc.Name);
            if (Equals(((MallocComboBoxItem) SettingsGameControlDropdownMalloc.SelectedItem).Malloc.Name, MALLOC_SYSTEM_DEFAULT)) return;
            SettingsGameControlCheckboxPages.IsChecked = false;
            STARTUP_HUGEPAGES = (bool) Core.SettingsHandler.WriteSetting("STARTUP_HUGEPAGES", false);
        }

        /// <summary>
        ///     Triggered when a startup/mod checkbox is clicked. Sets registry setting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SettingsGameControlCheckbox_Click(object sender, RoutedEventArgs args) {
            if (Equals(sender, SettingsGameControlCheckboxSplash)) {
                STARTUP_NOSPLASH = (bool) Core.SettingsHandler.WriteSetting("STARTUP_NOSPLASH", SettingsGameControlCheckboxSplash.IsChecked);
            } else if (Equals(sender, SettingsGameControlCheckboxWorld)) {
                STARTUP_EMPTYWORLD = (bool) Core.SettingsHandler.WriteSetting("STARTUP_EMPTYWORLD", SettingsGameControlCheckboxWorld.IsChecked);
            } else if (Equals(sender, SettingsGameControlCheckboxScript)) {
                STARTUP_SCRIPTERRORS = (bool) Core.SettingsHandler.WriteSetting("STARTUP_SCRIPTERRORS", SettingsGameControlCheckboxScript.IsChecked);
            } else if (Equals(sender, SettingsGameControlCheckboxPatching)) {
                STARTUP_FILEPATCHING = (bool) Core.SettingsHandler.WriteSetting("STARTUP_FILEPATCHING", SettingsGameControlCheckboxPatching.IsChecked);
            } else if (Equals(sender, SettingsGameControlCheckboxPages)) {
                STARTUP_HUGEPAGES = (bool) Core.SettingsHandler.WriteSetting("STARTUP_HUGEPAGES", SettingsGameControlCheckboxPages.IsChecked);
                SettingsGameControlDropdownMalloc.SelectedIndex = 0;
            } else if (Equals(sender, SettingsGameControlCheckboxShacktac)) {
                MODS_SHACKTAC = (bool) Core.SettingsHandler.WriteSetting("MODS_SHACKTAC", SettingsGameControlCheckboxShacktac.IsChecked);
            }
        }
    }
}