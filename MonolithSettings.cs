using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monolith
{
    public class MonolithSettings : ObservableObject
    {
        private string option1 = string.Empty;
        private bool option2 = false;
        private bool autoScanOnStartup = true;
        private List<string> scanDirectories = new List<string>();
        private List<string> skipList = new List<string>();

        public string Option1 { get => option1; set => SetValue(ref option1, value); }
        public bool Option2 { get => option2; set => SetValue(ref option2, value); }
        public bool AutoScanOnStartup { get => autoScanOnStartup; set => SetValue(ref autoScanOnStartup, value); }
        public List<string> ScanDirectories { get => scanDirectories; set => SetValue(ref scanDirectories, value); }
        public List<string> SkipList { get => skipList; set => SetValue(ref skipList, value); }

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.

    }

    public class MonolithSettingsViewModel : ObservableObject, ISettings
    {
        private readonly Monolith plugin;
        private MonolithSettings editingClone { get; set; }

        private MonolithSettings settings;
        public MonolithSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public MonolithSettingsViewModel(Monolith plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<MonolithSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new MonolithSettings();
            }
        }

        // Helper property for binding List<string> to a multiline TextBox
        public string ScanDirectoriesText
        {
            get => string.Join(Environment.NewLine, Settings.ScanDirectories);
            set
            {
                Settings.ScanDirectories = value
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                OnPropertyChanged();
            }
        }

        public string SkipListText
        {
            get => string.Join(Environment.NewLine, Settings.SkipList);
            set
            {
                Settings.SkipList = value
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                OnPropertyChanged();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
            // Notify UI that the text property has also changed (reverted)
            OnPropertyChanged(nameof(ScanDirectoriesText));
            OnPropertyChanged(nameof(SkipListText));
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}