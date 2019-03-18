using System.Collections.Generic;
using System.IO;
using System.Windows;
using UKSF.Launcher.Game;
using UKSF.Launcher.UI.General;

namespace UKSF.Launcher.UI.Main.Settings {
    public partial class SettingsGameControl {
        public static readonly RoutedEvent SETTINGS_GAME_CONTROL_WARNING_EVENT =
            EventManager.RegisterRoutedEvent("SETTINGS_GAME_CONTROL_WARNING_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(SettingsGameControl));

        public SettingsGameControl() {
            InitializeComponent();
        }

        public void Initialise() {
            AddHandler(ProfileSelectionControl.PROFILE_SELECTION_CONTROL_UPDATE_EVENT, new RoutedEventHandler(SettingsLauncherControlProfile_Update));
            AddHandler(SETTINGS_GAME_CONTROL_WARNING_EVENT, new RoutedEventHandler(SettingsLauncherControlProfile_Update));

            SettingsGameControlCheckboxSplash.IsChecked = Global.Settings.StartupNosplash;
            SettingsGameControlCheckboxScript.IsChecked = Global.Settings.StartupScripterrors;
            SettingsGameControlCheckboxPages.IsChecked = Global.Settings.StartupHugepages;

            SettingsGameControlDevControl.IsEnabled = false;
            SettingsGameControlCheckboxPatching.IsChecked = false;
#if DEV
            SettingsGameControlDevControl.IsEnabled = true;
            SettingsGameControlCheckboxPatching.IsChecked = Global.Settings.StartupFilepatching;
#endif

            if (SettingsGameControlDropdownMalloc.Items.IsEmpty) {
                AddMallocs();
            }

            UpdateWarning();
        }

        private void AddMallocs() {
            SettingsGameControlDropdownMalloc.Items.Clear();
            SettingsGameControlDropdownMalloc.Items.Add(new MallocComboBoxItem(Global.Constants.MALLOC_SYSTEM_DEFAULT, FindResource("Uksf.ComboBoxItem") as Style));
            List<MallocHandler.Malloc> mallocs = MallocHandler.GetMalloc(Path.Combine(Global.Settings.GameLocation, "..", "Dll"));
            foreach (MallocHandler.Malloc malloc in mallocs) {
                SettingsGameControlDropdownMalloc.Items.Add(new MallocComboBoxItem(malloc, FindResource("Uksf.ComboBoxItem") as Style));

                if (malloc.Name == Global.Settings.StartupMalloc) {
                    SettingsGameControlDropdownMalloc.SelectedIndex = mallocs.IndexOf(malloc);
                }
            }

            if (SettingsGameControlDropdownMalloc.SelectedIndex > -1) return;
            SettingsGameControlDropdownMalloc.SelectedIndex = 0;
        }

        private void SettingsLauncherControlProfile_Update(object sender, RoutedEventArgs args) {
            if (SettingsGameControlDropdownProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedItem != null) {
                if (Global.Settings.Profile != ((ProfileComboBoxItem) SettingsGameControlDropdownProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedItem)
                                               .Profile.DisplayName) {
                    Global.Settings.Profile = (string) Core.SettingsHandler.WriteSetting("PROFILE",
                                                                                         ((ProfileComboBoxItem) SettingsGameControlDropdownProfileSelectionControl
                                                                                                                .ProfileSelectionControlDropdownProfile.SelectedItem)
                                                                                         .Profile.DisplayName);
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

            SettingsGameControlProfileWarningText.Text = warning;
            if (MainWindow.Instance.HomeControl == null) return;
            MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.WarningRoutedEventArgs(HomeControl.HOME_CONTROL_WARNING_EVENT) {
                Warning = warning, Block = block, CurrentWarning = HomeControl.CurrentWarning.PROFILE
            });
        }

        private void SettingsLauncherControlMalloc_Update(object sender, RoutedEventArgs args) {
            if (SettingsGameControlDropdownMalloc.SelectedIndex == -1 ||
                Global.Settings.StartupMalloc == ((MallocComboBoxItem) SettingsGameControlDropdownMalloc.SelectedItem).Malloc.Name) {
                return;
            }

            Global.Settings.StartupMalloc =
                (string) Core.SettingsHandler.WriteSetting("STARTUP_MALLOC", ((MallocComboBoxItem) SettingsGameControlDropdownMalloc.SelectedItem).Malloc.Name);
            if (Equals(((MallocComboBoxItem) SettingsGameControlDropdownMalloc.SelectedItem).Malloc.Name, Global.Constants.MALLOC_SYSTEM_DEFAULT)) return;
            SettingsGameControlCheckboxPages.IsChecked = false;
            Global.Settings.StartupHugepages = (bool) Core.SettingsHandler.WriteSetting("STARTUP_HUGEPAGES", false);
        }

        private void SettingsGameControlCheckbox_Click(object sender, RoutedEventArgs args) {
            if (Equals(sender, SettingsGameControlCheckboxSplash)) {
                Global.Settings.StartupNosplash = (bool) Core.SettingsHandler.WriteSetting("STARTUP_NOSPLASH", SettingsGameControlCheckboxSplash.IsChecked);
            } else if (Equals(sender, SettingsGameControlCheckboxScript)) {
                Global.Settings.StartupScripterrors = (bool) Core.SettingsHandler.WriteSetting("STARTUP_SCRIPTERRORS", SettingsGameControlCheckboxScript.IsChecked);
            } else if (Equals(sender, SettingsGameControlCheckboxPatching)) {
                Global.Settings.StartupFilepatching = (bool) Core.SettingsHandler.WriteSetting("STARTUP_FILEPATCHING", SettingsGameControlCheckboxPatching.IsChecked);
            } else if (Equals(sender, SettingsGameControlCheckboxPages)) {
                Global.Settings.StartupHugepages = (bool) Core.SettingsHandler.WriteSetting("STARTUP_HUGEPAGES", SettingsGameControlCheckboxPages.IsChecked);
                SettingsGameControlDropdownMalloc.SelectedIndex = 0;
            }
        }
    }
}
