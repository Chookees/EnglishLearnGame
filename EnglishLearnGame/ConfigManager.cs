using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EnglishLearnGame
{
    /// <summary>
    /// Verwaltet alle Einstellungen aus der config.ini Datei
    /// </summary>
    public static class ConfigManager
    {
        private static readonly string configPath;
        private static Dictionary<string, string> configValues;

        static ConfigManager()
        {
            configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "config", "config.ini");
            configValues = new Dictionary<string, string>();
            LoadConfig();
        }

        /// <summary>
        /// Lädt die Konfiguration aus der config.ini Datei
        /// </summary>
        public static void LoadConfig()
        {
            configValues.Clear();
            
            // Standard-Werte setzen
            SetDefaultValues();
            
            // Config-Datei lesen falls vorhanden
            if (File.Exists(configPath))
            {
                string[] lines = File.ReadAllLines(configPath, Encoding.UTF8);
                string currentSection = "";
                
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    
                    // Leere Zeilen und Kommentare überspringen
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                        continue;
                    
                    // Sektion erkennen
                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                        continue;
                    }
                    
                    // Key-Value Paar parsen
                    if (trimmedLine.Contains("="))
                    {
                        int equalIndex = trimmedLine.IndexOf('=');
                        string key = trimmedLine.Substring(0, equalIndex).Trim();
                        string value = trimmedLine.Substring(equalIndex + 1).Trim();
                        
                        // Key mit Sektion kombinieren
                        string fullKey = string.IsNullOrEmpty(currentSection) ? key : $"{currentSection}.{key}";
                        configValues[fullKey] = value;
                    }
                }
            }
            else
            {
                // Config-Datei erstellen falls nicht vorhanden
                CreateDefaultConfig();
            }
        }

        /// <summary>
        /// Setzt Standard-Werte für alle Einstellungen
        /// </summary>
        private static void SetDefaultValues()
        {
            // GameSettings
            configValues["GameSettings.LanguageDirection"] = "0";
            configValues["GameSettings.DifficultyLevel"] = "A1";
            configValues["GameSettings.MaxErrorsPerLevel"] = "3";
            configValues["GameSettings.ShowHints"] = "true";
            configValues["GameSettings.SoundEnabled"] = "true";
            configValues["GameSettings.MusicEnabled"] = "true";
            
            // DisplaySettings
            configValues["DisplaySettings.Theme"] = "Light";
            configValues["DisplaySettings.FontSize"] = "14";
            configValues["DisplaySettings.ShowProgressBar"] = "true";
            configValues["DisplaySettings.ShowCharacterInfo"] = "true";
            
            // LearningSettings
            configValues["LearningSettings.RepeatFailedWords"] = "true";
            configValues["LearningSettings.RandomizeWordOrder"] = "true";
            configValues["LearningSettings.ShowWordFrequency"] = "true";
            configValues["LearningSettings.PracticeMode"] = "false";
            
            // Statistics
            configValues["Statistics.TotalWordsLearned"] = "0";
            configValues["Statistics.TotalTimeSpent"] = "0";
            configValues["Statistics.StreakDays"] = "0";
        }

        /// <summary>
        /// Erstellt eine Standard-Konfigurationsdatei
        /// </summary>
        private static void CreateDefaultConfig()
        {
            try
            {
                string configDir = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                string configContent = @"[GameSettings]
# Sprach-Einstellungen
LanguageDirection=0
# 0 = Deutsch → Englisch, 1 = Englisch → Deutsch

# Schwierigkeitsstufen (A1, A2, B1, B2, C1, C2)
DifficultyLevel=A1

# Spiel-Einstellungen
MaxErrorsPerLevel=3
ShowHints=true
SoundEnabled=true
MusicEnabled=true

[DisplaySettings]
# UI-Einstellungen
Theme=Light
FontSize=14
ShowProgressBar=true
ShowCharacterInfo=true

[LearningSettings]
# Lern-Einstellungen
RepeatFailedWords=true
RandomizeWordOrder=true
ShowWordFrequency=true
PracticeMode=false

[Statistics]
# Statistiken (werden automatisch verwaltet)
TotalWordsLearned=0
TotalTimeSpent=0
StreakDays=0";

                File.WriteAllText(configPath, configContent, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Erstellen der Config-Datei: {ex.Message}");
            }
        }

        /// <summary>
        /// Speichert die aktuelle Konfiguration
        /// </summary>
        public static void SaveConfig()
        {
            try
            {
                StringBuilder configContent = new StringBuilder();
                
                // GameSettings
                configContent.AppendLine("[GameSettings]");
                configContent.AppendLine("# Sprach-Einstellungen");
                configContent.AppendLine($"LanguageDirection={GetValue("GameSettings.LanguageDirection")}");
                configContent.AppendLine("# 0 = Deutsch → Englisch, 1 = Englisch → Deutsch");
                configContent.AppendLine();
                configContent.AppendLine("# Schwierigkeitsstufen (A1, A2, B1, B2, C1, C2)");
                configContent.AppendLine($"DifficultyLevel={GetValue("GameSettings.DifficultyLevel")}");
                configContent.AppendLine();
                configContent.AppendLine("# Spiel-Einstellungen");
                configContent.AppendLine($"MaxErrorsPerLevel={GetValue("GameSettings.MaxErrorsPerLevel")}");
                configContent.AppendLine($"ShowHints={GetValue("GameSettings.ShowHints")}");
                configContent.AppendLine($"SoundEnabled={GetValue("GameSettings.SoundEnabled")}");
                configContent.AppendLine($"MusicEnabled={GetValue("GameSettings.MusicEnabled")}");
                configContent.AppendLine();

                // DisplaySettings
                configContent.AppendLine("[DisplaySettings]");
                configContent.AppendLine("# UI-Einstellungen");
                configContent.AppendLine($"Theme={GetValue("DisplaySettings.Theme")}");
                configContent.AppendLine($"FontSize={GetValue("DisplaySettings.FontSize")}");
                configContent.AppendLine($"ShowProgressBar={GetValue("DisplaySettings.ShowProgressBar")}");
                configContent.AppendLine($"ShowCharacterInfo={GetValue("DisplaySettings.ShowCharacterInfo")}");
                configContent.AppendLine();

                // LearningSettings
                configContent.AppendLine("[LearningSettings]");
                configContent.AppendLine("# Lern-Einstellungen");
                configContent.AppendLine($"RepeatFailedWords={GetValue("LearningSettings.RepeatFailedWords")}");
                configContent.AppendLine($"RandomizeWordOrder={GetValue("LearningSettings.RandomizeWordOrder")}");
                configContent.AppendLine($"ShowWordFrequency={GetValue("LearningSettings.ShowWordFrequency")}");
                configContent.AppendLine($"PracticeMode={GetValue("LearningSettings.PracticeMode")}");
                configContent.AppendLine();

                // Statistics
                configContent.AppendLine("[Statistics]");
                configContent.AppendLine("# Statistiken (werden automatisch verwaltet)");
                configContent.AppendLine($"TotalWordsLearned={GetValue("Statistics.TotalWordsLearned")}");
                configContent.AppendLine($"TotalTimeSpent={GetValue("Statistics.TotalTimeSpent")}");
                configContent.AppendLine($"StreakDays={GetValue("Statistics.StreakDays")}");

                File.WriteAllText(configPath, configContent.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Speichern der Config-Datei: {ex.Message}");
            }
        }

        /// <summary>
        /// Holt einen Konfigurationswert
        /// </summary>
        /// <param name="key">Der Konfigurationsschlüssel</param>
        /// <param name="defaultValue">Standard-Wert falls nicht gefunden</param>
        /// <returns>Der Konfigurationswert</returns>
        public static string GetValue(string key, string defaultValue = "")
        {
            return configValues.ContainsKey(key) ? configValues[key] : defaultValue;
        }

        /// <summary>
        /// Setzt einen Konfigurationswert
        /// </summary>
        /// <param name="key">Der Konfigurationsschlüssel</param>
        /// <param name="value">Der neue Wert</param>
        public static void SetValue(string key, string value)
        {
            configValues[key] = value;
        }

        /// <summary>
        /// Holt einen booleschen Konfigurationswert
        /// </summary>
        public static bool GetBoolValue(string key, bool defaultValue = false)
        {
            string value = GetValue(key, defaultValue.ToString().ToLower());
            return bool.TryParse(value, out bool result) ? result : defaultValue;
        }

        /// <summary>
        /// Holt einen ganzzahligen Konfigurationswert
        /// </summary>
        public static int GetIntValue(string key, int defaultValue = 0)
        {
            string value = GetValue(key, defaultValue.ToString());
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        // Spezielle Getter für häufig verwendete Werte
        public static int LanguageDirection => GetIntValue("GameSettings.LanguageDirection", 0);
        public static string DifficultyLevel => GetValue("GameSettings.DifficultyLevel", "A1");
        public static int MaxErrorsPerLevel => GetIntValue("GameSettings.MaxErrorsPerLevel", 3);
        public static bool ShowHints => GetBoolValue("GameSettings.ShowHints", true);
        public static bool SoundEnabled => GetBoolValue("GameSettings.SoundEnabled", true);
        public static bool MusicEnabled => GetBoolValue("GameSettings.MusicEnabled", true);
        public static string Theme => GetValue("DisplaySettings.Theme", "Light");
        public static int FontSize => GetIntValue("DisplaySettings.FontSize", 14);
        public static bool ShowProgressBar => GetBoolValue("DisplaySettings.ShowProgressBar", true);
        public static bool ShowCharacterInfo => GetBoolValue("DisplaySettings.ShowCharacterInfo", true);
        public static bool RepeatFailedWords => GetBoolValue("LearningSettings.RepeatFailedWords", true);
        public static bool RandomizeWordOrder => GetBoolValue("LearningSettings.RandomizeWordOrder", true);
        public static bool ShowWordFrequency => GetBoolValue("LearningSettings.ShowWordFrequency", true);
        public static bool PracticeMode => GetBoolValue("LearningSettings.PracticeMode", false);
        public static int TotalWordsLearned => GetIntValue("Statistics.TotalWordsLearned", 0);
        public static int TotalTimeSpent => GetIntValue("Statistics.TotalTimeSpent", 0);
        public static int StreakDays => GetIntValue("Statistics.StreakDays", 0);
    }
}
