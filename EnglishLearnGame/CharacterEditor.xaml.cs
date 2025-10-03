using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace EnglishLearnGame
{
    /// <summary>
    /// Interaction logic for CharacterEditor.xaml
    /// </summary>
    public partial class CharacterEditor : Window
    {
        private string selectedCategory = "";
        private string selectedCharacter = "";
        private string characterName = "";
        private int characterAge = 0;
        private int characterClass = 0;
        
        private readonly string resourcesPath;
        private readonly string saveFilesPath;
        
        // Karussell-Status
        private Dictionary<string, int> carouselPositions = new Dictionary<string, int>();
        private Dictionary<string, List<string>> availableCharacters = new Dictionary<string, List<string>>();

        public CharacterEditor()
        {
            InitializeComponent();
            
            // Pfade zu den Ressourcen und Savefiles - korrigierte Pfade
            // Von bin\Debug\net8.0-windows\ zu D:\Projects\l\c#\EnglishLearnGame\
            resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "ressources", "char_editor");
            saveFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "savefiles");
            
            // Debug: Zeige den Pfad an
            System.Diagnostics.Debug.WriteLine($"Resources Path: {resourcesPath}");
            System.Diagnostics.Debug.WriteLine($"Resources Path exists: {Directory.Exists(resourcesPath)}");
            
            // Initialisiere Karussell-Positionen
            carouselPositions["human"] = 0;
            carouselPositions["heroes"] = 0;
            carouselPositions["monster"] = 0;
            
            // Lade die Charakter-Editor Ressourcen
            LoadCharacterAssets();
            
            // Setze Standard-Auswahl
            UpdateSelectionInfo();
        }

        private void LoadCharacterAssets()
        {
            try
            {
                // Lade alle Charakter-Kategorien
                LoadCharacterCategory("human", HumanCarousel);
                LoadCharacterCategory("heroes", HeroCarousel);
                LoadCharacterCategory("monster", MonsterCarousel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Charakter-Assets: {ex.Message}", 
                               "Fehler", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Error);
            }
        }
        
        private void LoadCharacterCategory(string categoryName, StackPanel carouselPanel)
        {
            string categoryPath = Path.Combine(resourcesPath, categoryName);
            availableCharacters[categoryName] = new List<string>();
            
            if (Directory.Exists(categoryPath))
            {
                string[] imageFiles = Directory.GetFiles(categoryPath, "*.png");
                foreach (string file in imageFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    availableCharacters[categoryName].Add(fileName);
                }
                
                System.Diagnostics.Debug.WriteLine($"Loaded {availableCharacters[categoryName].Count} characters for {categoryName}");
                
                // Erstelle Karussell für diese Kategorie
                CreateCarousel(categoryName, carouselPanel);
            }
        }
        
        private void CreateCarousel(string categoryName, StackPanel carouselPanel)
        {
            carouselPanel.Children.Clear();
            
            if (!availableCharacters.ContainsKey(categoryName) || availableCharacters[categoryName].Count == 0)
                return;
                
            var characters = availableCharacters[categoryName];
            int currentPos = carouselPositions[categoryName];
            
            // Zeige 5 Charaktere: 2 links, 1 mittig (groß), 2 rechts
            for (int i = -2; i <= 2; i++)
            {
                int index = (currentPos + i + characters.Count) % characters.Count;
                string characterName = characters[index];
                
                var characterButton = CreateCharacterButton(categoryName, characterName, i == 0);
                carouselPanel.Children.Add(characterButton);
            }
        }
        
        private Button CreateCharacterButton(string categoryName, string characterName, bool isCenter)
        {
            string imagePath = Path.Combine(resourcesPath, categoryName, $"{characterName}.png");
            
            var button = new Button
            {
                Tag = new { Category = categoryName, Character = characterName },
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(5),
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            
            if (File.Exists(imagePath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                
                var image = new System.Windows.Controls.Image
                {
                    Source = bitmap,
                    Stretch = System.Windows.Media.Stretch.Uniform
                };
                
                // Größe basierend auf Position
                if (isCenter)
                {
                    image.Width = 120;
                    image.Height = 120;
                    button.BorderBrush = System.Windows.Media.Brushes.Gold;
                    button.BorderThickness = new Thickness(3);
                }
                else
                {
                    image.Width = 80;
                    image.Height = 80;
                    button.Opacity = 0.7;
                }
                
                button.Content = image;
            }
            
            button.Click += (s, e) => {
                var tag = (dynamic)button.Tag;
                OnCharacterSelected(tag.Category, tag.Character);
            };
            
            return button;
        }

        private void OnCharacterSelected(string categoryName, string characterName)
        {
            selectedCategory = categoryName;
            selectedCharacter = characterName;
            
            System.Diagnostics.Debug.WriteLine($"Character selected: {categoryName}/{characterName}");
            
            // Aktualisiere die Karussell-Position für die ausgewählte Kategorie
            if (availableCharacters.ContainsKey(categoryName))
            {
                var characters = availableCharacters[categoryName];
                int characterIndex = characters.IndexOf(characterName);
                if (characterIndex >= 0)
                {
                    carouselPositions[categoryName] = characterIndex;
                    
                    // Aktualisiere das entsprechende Karussell
                    switch (categoryName.ToLower())
                    {
                        case "human":
                            CreateCarousel("human", HumanCarousel);
                            break;
                        case "heroes":
                            CreateCarousel("heroes", HeroCarousel);
                            break;
                        case "monster":
                            CreateCarousel("monster", MonsterCarousel);
                            break;
                    }
                }
            }
            
            UpdateSelectionInfo();
        }



        private void UpdateSelectionInfo()
        {
            if (string.IsNullOrEmpty(selectedCategory))
            {
                SelectedCategoryText.Text = "Kategorie: -";
                SelectedCharacterText.Text = "Charakter: -";
            }
            else
            {
                SelectedCategoryText.Text = $"Kategorie: {GetCategoryDisplayName(selectedCategory)}";
                SelectedCharacterText.Text = $"Charakter: {selectedCharacter}";
            }
            
            ValidateSaveButton();
        }
        
        private string GetCategoryDisplayName(string categoryName)
        {
            switch (categoryName.ToLower())
            {
                case "human": return "Menschen";
                case "heroes": return "Helden";
                case "monster": return "Monster";
                default: return categoryName;
            }
        }


        private void ValidateSaveButton()
        {
            bool canSave = !string.IsNullOrWhiteSpace(characterName) && 
                          characterAge > 0 && 
                          characterClass > 0 &&
                          !string.IsNullOrEmpty(selectedCategory) &&
                          !string.IsNullOrEmpty(selectedCharacter);
            
            SaveCharacterButton.IsEnabled = canSave;
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            characterName = NameTextBox.Text.Trim();
            ValidateSaveButton();
        }

        private void AgeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Verstecke/zeige Placeholder-Text
            AgePlaceholderText.Visibility = string.IsNullOrWhiteSpace(AgeTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
            
            if (int.TryParse(AgeTextBox.Text, out int age) && age > 0 && age <= 99)
            {
                characterAge = age;
                // Gültiges Alter - zeige normalen Tooltip
                AgeToolTipText.Text = "Gib ein Alter zwischen 1 und 99 ein";
                AgeToolTipText.Foreground = System.Windows.Media.Brushes.Black;
            }
            else
            {
                characterAge = 0;
                // Ungültiges Alter - zeige Warnung im Tooltip
                AgeToolTipText.Text = "❌ Nur Zahlen zwischen 1 und 99 sind gültig!";
                AgeToolTipText.Foreground = System.Windows.Media.Brushes.Red;
            }
            ValidateSaveButton();
        }

        private void AgeTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Markiere den gesamten Text beim Fokus
            AgeTextBox.SelectAll();
        }

        private void ClassTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Verstecke/zeige Placeholder-Text
            ClassPlaceholderText.Visibility = string.IsNullOrWhiteSpace(ClassTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
            
            if (int.TryParse(ClassTextBox.Text, out int classNum) && classNum > 0 && classNum <= 13)
            {
                characterClass = classNum;
                // Gültige Klasse - zeige normalen Tooltip
                ClassToolTipText.Text = "Gib eine Klasse zwischen 1 und 13 ein";
                ClassToolTipText.Foreground = System.Windows.Media.Brushes.Black;
            }
            else
            {
                characterClass = 0;
                // Ungültige Klasse - zeige Warnung im Tooltip
                ClassToolTipText.Text = "❌ Nur Zahlen zwischen 1 und 13 sind gültig!";
                ClassToolTipText.Foreground = System.Windows.Media.Brushes.Red;
            }
            ValidateSaveButton();
        }

        private void ClassTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Markiere den gesamten Text beim Fokus
            ClassTextBox.SelectAll();
        }

        private void SaveCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Generiere Hash aus Alter + Klasse
                string hash = GenerateHash(characterAge, characterClass);
                
                // Erstelle Charakter-Daten
                var characterData = new CharacterData
                {
                    Name = characterName,
                    Age = characterAge,
                    Class = characterClass,
                    Category = selectedCategory,
                    Character = selectedCharacter,
                    CreatedDate = DateTime.Now,
                    Hash = hash
                };
                
                // Speichere Charakter
                string fileName = $"{characterName}_{hash}.json";
                string filePath = Path.Combine(saveFilesPath, fileName);
                
                // Erstelle savefiles Ordner falls er nicht existiert
                if (!Directory.Exists(saveFilesPath))
                {
                    Directory.CreateDirectory(saveFilesPath);
                }
                
                // Serialisiere und speichere
                string json = System.Text.Json.JsonSerializer.Serialize(characterData, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(filePath, json);
                
                MessageBox.Show($"Charakter '{characterName}' wurde erfolgreich gespeichert!\n\nDatei: {fileName}", 
                               "Charakter gespeichert", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Information);
                
                // Schließe den Editor
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern des Charakters: {ex.Message}", 
                               "Fehler", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Error);
            }
        }

        private string GenerateHash(int age, int classNum)
        {
            // Erstelle einen Hash aus Alter + Klasse
            string input = $"{age}{classNum}{DateTime.Now.Ticks}";
            
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                
                // Konvertiere zu einem kürzeren String (erste 8 Zeichen)
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < 4; i++) // Nur erste 4 Bytes für kürzeren Hash
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                
                return builder.ToString();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Schließe den Editor
            this.Close();
        }

        /// <summary>
        /// Event-Handler für das Aufklappen der Expander - stellt sicher, dass nur einer geöffnet ist
        /// </summary>
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            var expandedExpander = sender as Expander;
            if (expandedExpander == null) return;

            // Prüfe ob alle Expander initialisiert sind, bevor wir sie verwenden
            if (HumanExpander == null || HeroExpander == null || MonsterExpander == null)
                return;

            // Schließe alle anderen Expander
            if (expandedExpander != HumanExpander)
                HumanExpander.IsExpanded = false;
            
            if (expandedExpander != HeroExpander)
                HeroExpander.IsExpanded = false;
            
            if (expandedExpander != MonsterExpander)
                MonsterExpander.IsExpanded = false;
        }
    }

    /// <summary>
    /// Klasse für Charakter-Daten
    /// </summary>
    public class CharacterData
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public int Class { get; set; }
        public string Category { get; set; } = "";
        public string Character { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public string Hash { get; set; } = "";
    }
}
