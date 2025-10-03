using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EnglishLearnGame
{
    /// <summary>
    /// Interaction logic for SaveFileSelectionWindow.xaml
    /// </summary>
    public partial class SaveFileSelectionWindow : Window
    {
        private List<CharacterData> characters;
        private readonly string resourcesPath;

        public CharacterData SelectedCharacter { get; private set; }

        public SaveFileSelectionWindow()
        {
            InitializeComponent();
            
            resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "ressources", "char_editor");
            
            LoadCharacters();
        }

        private void LoadCharacters()
        {
            try
            {
                characters = SaveFileManager.LoadAllCharacters();
                CharactersItemsControl.ItemsSource = characters;
                
                // Lade die Charakter-Bilder für die Vorschau
                LoadCharacterPreviews();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Charaktere: {ex.Message}", 
                               "Fehler", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Error);
            }
        }

        private void LoadCharacterPreviews()
        {
            // Die Bilder werden über DataTemplate geladen, aber wir müssen sicherstellen,
            // dass die Pfade korrekt sind
            foreach (var character in characters)
            {
                // Hier könnten wir die Bilder vorladen, aber das DataTemplate
                // wird sie automatisch laden wenn die Items angezeigt werden
            }
        }

        private void CharacterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CharacterData character)
            {
                SelectedCharacter = character;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void NewCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            // Öffne den Charakter-Editor für einen neuen Charakter
            CharacterEditor characterEditor = new CharacterEditor();
            characterEditor.Owner = this;
            
            if (characterEditor.ShowDialog() == true)
            {
                // Charakter wurde erstellt, lade die Liste neu
                LoadCharacters();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    /// <summary>
    /// Converter für die Charakter-Bilder in der Auswahl
    /// </summary>
    public class CharacterImageConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is CharacterData character && parameter is string imageType)
            {
                string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "ressources", "char_editor");
                string imagePath = "";
                
                switch (imageType.ToLower())
                {
                    case "character":
                        imagePath = Path.Combine(resourcesPath, character.Category, $"{character.Character}.png");
                        break;
                }
                
                if (File.Exists(imagePath))
                {
                    return new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                }
            }
            
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
