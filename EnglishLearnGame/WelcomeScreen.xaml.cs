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
                characterEditor.ShowDialog();
                
                // After character editor is closed, reload characters and check if we should start the game
                LoadAvailableCharacters();
                
                // If exactly one character was created, start the game immediately
                if (availableCharacters.Count == 1)
                {
                    LoadCharacter(availableCharacters[0]);
                }
            }
            else
            {
                // Always show selection window when "Fortfahren" is clicked
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

        private void NewCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            // Open character editor for new character
            CharacterEditor characterEditor = new CharacterEditor();
            characterEditor.Owner = this;
            characterEditor.ShowDialog();
            
            // After character editor is closed, reload characters
            LoadAvailableCharacters();
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
                NewCharacterButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Save files exist - show "Continue" and "Neu" button
                StartButton.Content = "▶️ Fortfahren";
                CharacterInfoPanel.Visibility = Visibility.Collapsed;
                NewCharacterButton.Visibility = Visibility.Visible;
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
