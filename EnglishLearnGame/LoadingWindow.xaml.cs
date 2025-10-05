using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EnglishLearnGame
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        private CharacterData characterData;

        public LoadingWindow(CharacterData character)
        {
            InitializeComponent();
            characterData = character;
            LoadSettingsAsync();
        }

        /// <summary>
        /// Lädt alle Einstellungen asynchron
        /// </summary>
        private async void LoadSettingsAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    // Schritt 1: Config laden
                    await UpdateUI("Lade Konfiguration...", "Lese config.ini Datei...", 10);
                    ConfigManager.LoadConfig();
                    await Task.Delay(500);

                    // Schritt 2: Charakter-Einstellungen laden
                    await UpdateUI("Lade Charakter-Einstellungen...", "Verarbeite Spielerdaten...", 30);
                    await LoadCharacterSettings();
                    await Task.Delay(500);

                    // Schritt 3: Spielstand laden
                    await UpdateUI("Lade Spielstand...", "Verarbeite Lernstatistiken...", 50);
                    await LoadGameProgress();
                    await Task.Delay(500);

                    // Schritt 4: Vokabeln laden
                    await UpdateUI("Lade Vokabeln...", "Lade CSV-Dateien für Schwierigkeitsstufe...", 70);
                    await LoadVocabulary();
                    await Task.Delay(500);

                    // Schritt 5: UI vorbereiten
                    await UpdateUI("Bereite Spiel vor...", "Initialisiere Benutzeroberfläche...", 90);
                    await Task.Delay(500);

                    // Schritt 6: Fertig
                    await UpdateUI("Fertig!", "Starte Vokabel-Test...", 100);
                    await Task.Delay(1000);
                });

                // Main Game öffnen
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        MainGameWindow mainGame = new MainGameWindow(characterData);
                        mainGame.Owner = this;
                        bool? result = mainGame.ShowDialog();
                        
                        // Wenn MainGameWindow wegen Fehler geschlossen wurde, zurück zum Hauptmenü
                        if (result == false)
                        {
                            // Zurück zum Hauptmenü (WelcomeScreen)
                            WelcomeScreen welcomeScreen = new WelcomeScreen();
                            welcomeScreen.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fehler beim Öffnen des Spiels: {ex.Message}", 
                                       "Fehler", 
                                       MessageBoxButton.OK, 
                                       MessageBoxImage.Error);
                        
                        // Zurück zum Hauptmenü
                        WelcomeScreen welcomeScreen = new WelcomeScreen();
                        welcomeScreen.Show();
                    }
                    finally
                    {
                        this.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Fehler beim Laden der Einstellungen: {ex.Message}", 
                                   "Fehler", 
                                   MessageBoxButton.OK, 
                                   MessageBoxImage.Error);
                    this.Close();
                });
            }
        }

        /// <summary>
        /// Aktualisiert die UI-Elemente
        /// </summary>
        private async Task UpdateUI(string status, string detail, int progress)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                StatusText.Text = status;
                DetailText.Text = detail;
                ProgressBar.Value = progress;
                ProgressText.Text = $"{progress}%";
            });
        }

        /// <summary>
        /// Lädt Charakter-spezifische Einstellungen
        /// </summary>
        private async Task LoadCharacterSettings()
        {
            await Task.Run(() =>
            {
                // Charakter-Einstellungen aus Config übernehmen
                if (characterData != null)
                {
                    characterData.CurrentDifficulty = ConfigManager.DifficultyLevel;
                    characterData.LanguageDirection = ConfigManager.LanguageDirection;
                }
            });
        }

        /// <summary>
        /// Lädt den Spielstand
        /// </summary>
        private async Task LoadGameProgress()
        {
            await Task.Run(() =>
            {
                // Spielstand wird bereits im Charakter-Savefile gespeichert
                // Hier könnten zusätzliche Spielstand-Daten geladen werden
                if (characterData != null && characterData.LevelStats == null)
                {
                    characterData.LevelStats = new Dictionary<string, LevelStatistics>();
                }
            });
        }

        /// <summary>
        /// Lädt Vokabeln für die aktuelle Schwierigkeitsstufe
        /// </summary>
        private async Task LoadVocabulary()
        {
            await Task.Run(() =>
            {
                // Vokabeln werden im MainGameWindow geladen
                // Hier wird nur vorbereitet
                string difficulty = characterData?.CurrentDifficulty ?? ConfigManager.DifficultyLevel;
                System.Diagnostics.Debug.WriteLine($"Lade Vokabeln für Schwierigkeitsstufe: {difficulty}");
            });
        }
    }
}
