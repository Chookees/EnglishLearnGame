using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace EnglishLearnGame
{
    /// <summary>
    /// Interaction logic for WelcomeScreen.xaml
    /// </summary>
    public partial class WelcomeScreen : Window
    {
        private readonly string saveFilesPath;
        private List<CharacterData> availableCharacters;

        public WelcomeScreen()
        {
            InitializeComponent();
            
            // Pfad zum savefiles Ordner relativ zum Projekt
            saveFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "savefiles");
            
            // Lade verfügbare Charaktere und aktualisiere UI
            LoadAvailableCharacters();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (availableCharacters.Count == 0)
            {
                // Kein Spielstand - öffne Charakter-Editor für neuen Spieler
                CharacterEditor characterEditor = new CharacterEditor();
                characterEditor.Owner = this;
                characterEditor.Closed += (s, e) => {
                    // Lade Charaktere neu wenn der Editor geschlossen wird
                    LoadAvailableCharacters();
                };
                characterEditor.Show();
            }
            else if (availableCharacters.Count == 1)
            {
                // Ein Spielstand - lade direkt
                LoadCharacter(availableCharacters[0]);
            }
            else
            {
                // Mehrere Spielstände - zeige Auswahl
                SaveFileSelectionWindow selectionWindow = new SaveFileSelectionWindow();
                selectionWindow.Owner = this;
                
                if (selectionWindow.ShowDialog() == true && selectionWindow.SelectedCharacter != null)
                {
                    LoadCharacter(selectionWindow.SelectedCharacter);
                }
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Öffne Einstellungsfenster
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Beende das Programm komplett
            var result = MessageBox.Show("Möchtest du das Spiel wirklich beenden?", 
                                        "Spiel beenden", 
                                        MessageBoxButton.YesNo, 
                                        MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Lädt alle verfügbaren Charaktere und aktualisiert die UI
        /// </summary>
        private void LoadAvailableCharacters()
        {
            try
            {
                availableCharacters = SaveFileManager.LoadAllCharacters();
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Charaktere: {ex.Message}", 
                               "Fehler", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Error);
                availableCharacters = new List<CharacterData>();
                UpdateUI();
            }
        }

        /// <summary>
        /// Aktualisiert die UI basierend auf verfügbaren Charakteren
        /// </summary>
        private void UpdateUI()
        {
            if (availableCharacters.Count == 0)
            {
                // Kein Spielstand - zeige "Start"
                StartButton.Content = "🚀 Start";
                CharacterInfoPanel.Visibility = Visibility.Collapsed;
            }
            else if (availableCharacters.Count == 1)
            {
                // Ein Spielstand - zeige "Fortfahren" mit Charakterdaten
                StartButton.Content = "▶️ Fortfahren";
                CharacterInfoPanel.Visibility = Visibility.Visible;
                
                var character = availableCharacters[0];
                CharacterNameDisplay.Text = character.Name;
                CharacterDetailsDisplay.Text = $"Alter: {character.Age} | Klasse: {character.Class}";
            }
            else
            {
                // Mehrere Spielstände - zeige "Fortfahren"
                StartButton.Content = "▶️ Fortfahren";
                CharacterInfoPanel.Visibility = Visibility.Collapsed;
            }
        }

                /// <summary>
                /// Lädt einen Charakter und startet das Spiel
                /// </summary>
                /// <param name="character">Der zu ladende Charakter</param>
                private void LoadCharacter(CharacterData character)
                {
                    try
                    {
                        // Öffne Loading Window
                        LoadingWindow loadingWindow = new LoadingWindow(character);
                        loadingWindow.Owner = this;
                        loadingWindow.Show();
                        
                        // Schließe Welcome Screen
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fehler beim Laden des Charakters: {ex.Message}", 
                                       "Fehler", 
                                       MessageBoxButton.OK, 
                                       MessageBoxImage.Error);
                    }
                }

        /// <summary>
        /// Prüft ob bereits Savefiles im savefiles Ordner existieren
        /// </summary>
        /// <returns>True wenn Savefiles gefunden wurden, sonst False</returns>
        private bool HasExistingSaveFiles()
        {
            return availableCharacters.Count > 0;
        }
    }
}
