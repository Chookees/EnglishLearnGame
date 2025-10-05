using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace EnglishLearnGame
{
    /// <summary>
    /// Interaction logic for MainGameWindow.xaml
    /// </summary>
    public partial class MainGameWindow : Window
    {
        private CharacterData characterData;
        private List<VocabularyWord> vocabularyWords;
        private VocabularyWord currentWord;
        private int correctAnswers = 0;
        private int totalWords = 0;
        private int currentStreak = 0;
        private DateTime sessionStartTime;
        private DispatcherTimer gameTimer;
        private string resourcesPath;
        private string saveFilesPath;

        public MainGameWindow(CharacterData character)
        {
            InitializeComponent();
            characterData = character;
            
            // Initialize paths
            resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "ressources");
            saveFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "savefiles");
            
            InitializeGame();
        }

        /// <summary>
        /// Initializes the game
        /// </summary>
        private void InitializeGame()
        {
            try
            {
                // Show character info
                LoadCharacterInfo();
                
                // Load vocabulary
                LoadVocabulary();
                
                // Update UI
                UpdateUI();
                
                // Start timer
                sessionStartTime = DateTime.Now;
                gameTimer = new DispatcherTimer();
                gameTimer.Interval = TimeSpan.FromSeconds(1);
                gameTimer.Tick += GameTimer_Tick;
                gameTimer.Start();
                
                // Show first word
                ShowNextWord();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing game: {ex.Message}", 
                               "Error", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads character information
        /// </summary>
        private void LoadCharacterInfo()
        {
            if (characterData == null) return;

            // Load character image
            string characterPath = Path.Combine(resourcesPath, "char_editor", characterData.Category, $"{characterData.Character}.png");
            if (File.Exists(characterPath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(characterPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                CharacterImage.Source = bitmap;
            }

            // Show character details
            CharacterNameText.Text = characterData.Name;
            CharacterDetailsText.Text = $"Alter: {characterData.Age} | Klasse: {characterData.Class}";

            // Load statistics
            LoadStatistics();
        }

        /// <summary>
        /// Loads statistics for the current difficulty level
        /// </summary>
        private void LoadStatistics()
        {
            StatsPanel.Children.Clear();
            
            string difficulty = characterData.CurrentDifficulty;
            if (characterData.LevelStats.ContainsKey(difficulty))
            {
                var stats = characterData.LevelStats[difficulty];
                
                AddStatRow("✅ Richtig:", stats.CorrectAnswers.ToString());
                AddStatRow("❌ Falsch:", stats.WrongAnswers.ToString());
                AddStatRow("🔥 Beste Serie:", stats.BestStreak.ToString());
                AddStatRow("📅 Zuletzt:", stats.LastPlayed.ToString("dd.MM.yyyy"));
            }
            else
            {
                AddStatRow("📚 Neue Stufe", "Noch keine Daten");
            }
        }

        /// <summary>
        /// Fügt eine Statistik-Zeile hinzu
        /// </summary>
        private void AddStatRow(string label, string value)
        {
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            stackPanel.Children.Add(new TextBlock { Text = label, FontSize = 12, Width = 100 });
            stackPanel.Children.Add(new TextBlock { Text = value, FontSize = 12, FontWeight = FontWeights.Bold });
            StatsPanel.Children.Add(stackPanel);
        }

        /// <summary>
        /// Lädt Vokabeln für die aktuelle Schwierigkeitsstufe
        /// </summary>
        private void LoadVocabulary()
        {
            vocabularyWords = new List<VocabularyWord>();
            string difficulty = characterData?.CurrentDifficulty ?? ConfigManager.DifficultyLevel;
            
            // Try multiple possible paths for the CSV file
            string[] possiblePaths = {
                Path.Combine(resourcesPath, "vocabulary", $"{difficulty}.csv"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "ressources", "vocabulary", $"{difficulty}.csv"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "ressources", "vocabulary", $"{difficulty}.csv"),
                Path.Combine(Directory.GetCurrentDirectory(), "ressources", "vocabulary", $"{difficulty}.csv"),
                Path.Combine(Environment.CurrentDirectory, "ressources", "vocabulary", $"{difficulty}.csv")
            };
            
            string csvPath = null;
            foreach (string path in possiblePaths)
            {
                string fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    csvPath = fullPath;
                    break;
                }
            }
            
            // Debug output
            System.Diagnostics.Debug.WriteLine($"Difficulty: {difficulty}");
            System.Diagnostics.Debug.WriteLine($"CSV Path found: {csvPath ?? "NONE"}");
            
            if (!string.IsNullOrEmpty(csvPath) && File.Exists(csvPath))
            {
                string[] lines = File.ReadAllLines(csvPath);
                System.Diagnostics.Debug.WriteLine($"CSV lines loaded: {lines.Length}");
                
                foreach (string line in lines.Skip(1)) // Header überspringen
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        // Try both comma and semicolon as separators
                        string[] parts = line.Split(',');
                        if (parts.Length < 2)
                        {
                            parts = line.Split(';');
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"Line: '{line}' -> Parts: {parts.Length}");
                        if (parts.Length >= 2)
                        {
                            vocabularyWords.Add(new VocabularyWord
                            {
                                German = parts[0].Trim(),
                                English = parts[1].Trim()
                            });
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"CSV file not found for difficulty: {difficulty}");
                System.Diagnostics.Debug.WriteLine("Tried paths:");
                foreach (string path in possiblePaths)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {Path.GetFullPath(path)}");
                }
            }
            
            totalWords = vocabularyWords.Count;
            System.Diagnostics.Debug.WriteLine($"Total vocabulary words loaded: {totalWords}");
            
            // Vokabeln mischen falls aktiviert
            if (ConfigManager.RandomizeWordOrder)
            {
                vocabularyWords = vocabularyWords.OrderBy(x => Guid.NewGuid()).ToList();
            }
        }

        /// <summary>
        /// Aktualisiert die UI
        /// </summary>
        private void UpdateUI()
        {
            string difficulty = characterData?.CurrentDifficulty ?? ConfigManager.DifficultyLevel;
            DifficultyText.Text = $"Schwierigkeitsstufe: {difficulty}";
            ProgressText.Text = $"{correctAnswers} / {totalWords}";
            StreakText.Text = $"🔥 {currentStreak}";
        }

        /// <summary>
        /// Zeigt das nächste Wort an
        /// </summary>
        private void ShowNextWord()
        {
            if (vocabularyWords.Count == 0)
            {
                if (totalWords == 0)
                {
                    // Keine Vokabeln geladen - Fehler
                    string errorMessage = $"Fehler: Keine Vokabeln für Schwierigkeit '{characterData?.CurrentDifficulty ?? ConfigManager.DifficultyLevel}' gefunden!\n\n" +
                                        "Bitte überprüfe, ob die CSV-Datei existiert und korrekt formatiert ist.\n\n" +
                                        "Debug-Informationen findest du im Output-Fenster deiner IDE.";
                    
                    MessageBox.Show(errorMessage,
                                   "Fehler beim Laden der Vokabeln",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Error);
                    
                    // Window schließen und zurück zum Hauptmenü
                    this.DialogResult = false;
                    return;
                }
                else
                {
                    // Alle Vokabeln bearbeitet
                    ShowLevelCompleteMessage();
                    return;
                }
            }

            // Zufälliges Wort auswählen
            var random = new Random();
            int index = random.Next(vocabularyWords.Count);
            currentWord = vocabularyWords[index];
            vocabularyWords.RemoveAt(index);

            // Wort anzeigen
            bool isGermanToEnglish = ConfigManager.LanguageDirection == 0;
            WordText.Text = isGermanToEnglish ? currentWord.German : currentWord.English;
            
            // UI zurücksetzen
            AnswerTextBox.Text = "";
            AnswerTextBox.IsEnabled = true;
            CheckButton.IsEnabled = false;
            FeedbackText.Visibility = Visibility.Collapsed;
            AnswerTextBox.Background = System.Windows.Media.Brushes.White;
            
            // Fokus setzen
            AnswerTextBox.Focus();
        }

        /// <summary>
        /// Prüft die Antwort
        /// </summary>
        private void CheckAnswer()
        {
            if (currentWord == null || string.IsNullOrWhiteSpace(AnswerTextBox.Text))
                return;

            string userAnswer = AnswerTextBox.Text.Trim().ToLower();
            bool isGermanToEnglish = ConfigManager.LanguageDirection == 0;
            string correctAnswer = (isGermanToEnglish ? currentWord.English : currentWord.German).ToLower();
            
            bool isCorrect = string.Equals(userAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase);
            
            if (isCorrect)
            {
                correctAnswers++;
                currentStreak++;
                ShowFeedback("✅ Richtig!", System.Windows.Media.Brushes.Green);
                SaveProgress(true);
            }
            else
            {
                currentStreak = 0; // Streak zurücksetzen bei Fehler
                ShowFeedback($"❌ Falsch! Richtige Antwort: {correctAnswer}", System.Windows.Media.Brushes.Red);
                SaveProgress(false);
            }
            
            UpdateUI();
            
            // Nach kurzer Pause nächste Frage
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                ShowNextWord();
            };
            timer.Start();
        }

        /// <summary>
        /// Zeigt Feedback an
        /// </summary>
        private void ShowFeedback(string message, System.Windows.Media.Brush color)
        {
            FeedbackText.Text = message;
            FeedbackText.Foreground = color;
            FeedbackText.Visibility = Visibility.Visible;
            AnswerTextBox.IsEnabled = false;
            CheckButton.IsEnabled = false;
        }

        /// <summary>
        /// Speichert den Fortschritt
        /// </summary>
        private void SaveProgress(bool isCorrect)
        {
            try
            {
                string difficulty = characterData.CurrentDifficulty;
                
                if (!characterData.LevelStats.ContainsKey(difficulty))
                {
                    characterData.LevelStats[difficulty] = new LevelStatistics();
                }
                
                var stats = characterData.LevelStats[difficulty];
                
                if (isCorrect)
                {
                    stats.CorrectAnswers++;
                    if (stats.CurrentStreak + 1 > stats.BestStreak)
                    {
                        stats.BestStreak = stats.CurrentStreak + 1;
                    }
                    stats.CurrentStreak++;
                }
                else
                {
                    stats.WrongAnswers++;
                    stats.CurrentStreak = 0;
                }
                
                stats.TotalWords++;
                stats.LastPlayed = DateTime.Now;
                
                // Charakter-Datei speichern
                string fileName = $"{characterData.Name}_{characterData.Hash}.json";
                string filePath = Path.Combine(saveFilesPath, fileName);
                string json = JsonSerializer.Serialize(characterData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Speichern: {ex.Message}");
            }
        }

        /// <summary>
        /// Zeigt die Level-Abschluss-Nachricht
        /// </summary>
        private void ShowLevelCompleteMessage()
        {
            gameTimer.Stop();
            
            string difficulty = characterData.CurrentDifficulty;
            var stats = characterData.LevelStats.ContainsKey(difficulty) ? characterData.LevelStats[difficulty] : new LevelStatistics();
            
            string message = $"🎉 Gratulation {characterData.Name}! 🎉\n\n" +
                           $"Du hast alle Vokabeln der Stufe {difficulty} erfolgreich übersetzt!\n\n" +
                           $"📊 Deine Statistiken:\n" +
                           $"• Richtige Antworten: {stats.CorrectAnswers}\n" +
                           $"• Falsche Antworten: {stats.WrongAnswers}\n" +
                           $"• Beste Serie: {stats.BestStreak}\n\n" +
                           $"🎯 **Empfehlung:**\n" +
                           $"Du solltest jetzt die nächste Schwierigkeitsstufe in den Einstellungen einstellen!\n\n" +
                           $"Möchtest du weiter üben oder zurück zum Menü?";

            MessageBoxResult result = MessageBox.Show(message, 
                                                    "Level abgeschlossen!", 
                                                    MessageBoxButton.YesNo, 
                                                    MessageBoxImage.Information);
            
            if (result == MessageBoxResult.Yes)
            {
                // Weiter üben - neues Level
                LoadVocabulary();
                correctAnswers = 0;
                currentStreak = 0;
                UpdateUI();
                ShowNextWord();
                gameTimer.Start();
            }
            else
            {
                // Zurück zum Menü
                this.DialogResult = true;
            }
        }

        // Event-Handler
        private void AnswerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckButton.IsEnabled = !string.IsNullOrWhiteSpace(AnswerTextBox.Text);
        }

        private void AnswerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && CheckButton.IsEnabled)
            {
                CheckAnswer();
            }
        }

        private void AnswerTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            AnswerTextBox.SelectAll();
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            CheckAnswer();
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            gameTimer?.Stop();
            this.DialogResult = true;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            var elapsed = DateTime.Now - sessionStartTime;
            TimeText.Text = elapsed.ToString(@"mm\:ss");
        }

        protected override void OnClosed(EventArgs e)
        {
            gameTimer?.Stop();
            base.OnClosed(e);
        }
    }

    /// <summary>
    /// Klasse für Vokabeln
    /// </summary>
    public class VocabularyWord
    {
        public string German { get; set; } = "";
        public string English { get; set; } = "";
    }
}
