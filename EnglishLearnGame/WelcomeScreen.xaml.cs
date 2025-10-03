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
            
            // Path to savefiles folder relative to project
            saveFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "savefiles");
            
            // Load available characters and update UI
            LoadAvailableCharacters();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (availableCharacters.Count == 0)
            {
                // No save file - open character editor for new player
                CharacterEditor characterEditor = new CharacterEditor();
                characterEditor.Owner = this;
                characterEditor.Closed += (s, e) => {
                    // Reload characters when editor is closed
                    LoadAvailableCharacters();
                };
                characterEditor.Show();
            }
            else if (availableCharacters.Count == 1)
            {
                // One save file - load directly
                LoadCharacter(availableCharacters[0]);
            }
            else
            {
                // Multiple save files - show selection
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
            // Open settings window
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Exit program completely
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
        /// Loads all available characters and updates the UI
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
                MessageBox.Show($"Error loading characters: {ex.Message}", 
                               "Error", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Error);
                availableCharacters = new List<CharacterData>();
                UpdateUI();
            }
        }

        /// <summary>
        /// Updates UI based on available characters
        /// </summary>
        private void UpdateUI()
        {
            if (availableCharacters.Count == 0)
            {
                // No save file - show "Start"
                StartButton.Content = "🚀 Start";
                CharacterInfoPanel.Visibility = Visibility.Collapsed;
            }
            else if (availableCharacters.Count == 1)
            {
                // One save file - show "Continue" with character data
                StartButton.Content = "▶️ Fortfahren";
                CharacterInfoPanel.Visibility = Visibility.Visible;
                
                var character = availableCharacters[0];
                CharacterNameDisplay.Text = character.Name;
                CharacterDetailsDisplay.Text = $"Alter: {character.Age} | Klasse: {character.Class}";
            }
            else
            {
                // Multiple save files - show "Continue"
                StartButton.Content = "▶️ Fortfahren";
                CharacterInfoPanel.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Loads a character and starts the game
        /// </summary>
        /// <param name="character">The character to load</param>
                private void LoadCharacter(CharacterData character)
                {
                    try
                    {
                        // Open Loading Window
                        LoadingWindow loadingWindow = new LoadingWindow(character);
                        loadingWindow.Owner = this;
                        loadingWindow.Show();
                        
                        // Close Welcome Screen
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading character: {ex.Message}", 
                                       "Error", 
                                       MessageBoxButton.OK, 
                                       MessageBoxImage.Error);
                    }
                }

        /// <summary>
        /// Checks if savefiles already exist in the savefiles folder
        /// </summary>
        /// <returns>True if savefiles were found, otherwise False</returns>
        private bool HasExistingSaveFiles()
        {
            return availableCharacters.Count > 0;
        }
    }
}
