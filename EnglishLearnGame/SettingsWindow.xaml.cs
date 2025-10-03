using System;
using System.Windows;
using System.Windows.Controls;

namespace EnglishLearnGame
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            
            // Event-Handler für Volume Slider
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            
            // Lade gespeicherte Einstellungen (falls vorhanden)
            LoadSettings();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumeLabel != null)
            {
                VolumeLabel.Text = $"{(int)e.NewValue}%";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // TODO: Hier werden später die Einstellungen in eine Konfigurationsdatei gespeichert
                // Für jetzt zeigen wir nur eine Bestätigung
                
                MessageBox.Show("Einstellungen wurden gespeichert!", 
                               "Einstellungen", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Information);
                
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern der Einstellungen: {ex.Message}", 
                               "Fehler", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Lädt die gespeicherten Einstellungen
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // TODO: Hier werden später die Einstellungen aus einer Konfigurationsdatei geladen
                // Für jetzt verwenden wir Standardwerte
                
                // Standardwerte setzen
                SoundEnabledCheckBox.IsChecked = true;
                MusicEnabledCheckBox.IsChecked = true;
                VolumeSlider.Value = 70;
                VolumeLabel.Text = "70%";
                AutoSaveCheckBox.IsChecked = true;
                ShowHintsCheckBox.IsChecked = true;
                FullScreenCheckBox.IsChecked = false;
                
                // Standard-Schwierigkeitsgrad auf A1 setzen
                DifficultyComboBox.SelectedIndex = 0;
                WindowSizeComboBox.SelectedIndex = 0;
                
                // Standard-Sprachrichtung auf Deutsch → Englisch setzen
                LanguageDirectionComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Einstellungen: {ex.Message}", 
                               "Fehler", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Gibt die aktuellen Einstellungen als Objekt zurück
        /// </summary>
        /// <returns>Settings-Objekt mit aktuellen Werten</returns>
        public GameSettings GetCurrentSettings()
        {
            return new GameSettings
            {
                SoundEnabled = SoundEnabledCheckBox.IsChecked ?? true,
                MusicEnabled = MusicEnabledCheckBox.IsChecked ?? true,
                Volume = (int)VolumeSlider.Value,
                DefaultDifficulty = DifficultyComboBox.SelectedIndex,
                AutoSave = AutoSaveCheckBox.IsChecked ?? true,
                ShowHints = ShowHintsCheckBox.IsChecked ?? true,
                FullScreen = FullScreenCheckBox.IsChecked ?? false,
                WindowSize = WindowSizeComboBox.SelectedIndex,
                LanguageDirection = LanguageDirectionComboBox.SelectedIndex
            };
        }
    }

    /// <summary>
    /// Klasse für die Spiel-Einstellungen
    /// </summary>
    public class GameSettings
    {
        public bool SoundEnabled { get; set; } = true;
        public bool MusicEnabled { get; set; } = true;
        public int Volume { get; set; } = 70;
        public int DefaultDifficulty { get; set; } = 0; // 0 = A1, 1 = A2, etc.
        public bool AutoSave { get; set; } = true;
        public bool ShowHints { get; set; } = true;
        public bool FullScreen { get; set; } = false;
        public int WindowSize { get; set; } = 0; // 0 = Klein, 1 = Mittel, etc.
        public int LanguageDirection { get; set; } = 0; // 0 = Deutsch→Englisch, 1 = Englisch→Deutsch
        
        /// <summary>
        /// Gibt die aktuelle Sprachrichtung als lesbaren String zurück
        /// </summary>
        public string GetLanguageDirectionText()
        {
            return LanguageDirection == 0 ? "Deutsch → Englisch" : "Englisch → Deutsch";
        }
        
        /// <summary>
        /// Prüft ob die Lernrichtung Deutsch → Englisch ist
        /// </summary>
        public bool IsGermanToEnglish()
        {
            return LanguageDirection == 0;
        }
    }
}
