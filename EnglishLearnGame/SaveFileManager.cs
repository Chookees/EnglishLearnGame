using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EnglishLearnGame
{
    /// <summary>
    /// Manager für Savefile-Operationen
    /// </summary>
    public static class SaveFileManager
    {
        /// <summary>
        /// Lädt alle verfügbaren Charakter-Savefiles
        /// </summary>
        /// <returns>Liste aller Charakterdaten</returns>
        public static List<CharacterData> LoadAllCharacters()
        {
            var characters = new List<CharacterData>();
            string saveFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "savefiles");
            
            try
            {
                if (!Directory.Exists(saveFilesPath))
                {
                    return characters;
                }

                string[] jsonFiles = Directory.GetFiles(saveFilesPath, "*.json");
                
                foreach (string filePath in jsonFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(filePath);
                        var character = JsonSerializer.Deserialize<CharacterData>(json);
                        if (character != null)
                        {
                            characters.Add(character);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Logge Fehler beim Laden einzelner Dateien, aber breche nicht ab
                        System.Diagnostics.Debug.WriteLine($"Fehler beim Laden von {filePath}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Savefiles: {ex.Message}");
            }
            
            return characters;
        }
        
        /// <summary>
        /// Lädt einen spezifischen Charakter anhand des Dateinamens
        /// </summary>
        /// <param name="fileName">Name der Savefile</param>
        /// <returns>Charakterdaten oder null wenn nicht gefunden</returns>
        public static CharacterData LoadCharacter(string fileName)
        {
            string saveFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "savefiles");
            string filePath = Path.Combine(saveFilesPath, fileName);
            
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<CharacterData>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Laden von {fileName}: {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Prüft ob Savefiles existieren
        /// </summary>
        /// <returns>Anzahl der gefundenen Savefiles</returns>
        public static int GetSaveFileCount()
        {
            string saveFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "savefiles");
            
            try
            {
                if (!Directory.Exists(saveFilesPath))
                {
                    return 0;
                }

                string[] jsonFiles = Directory.GetFiles(saveFilesPath, "*.json");
                return jsonFiles.Length;
            }
            catch
            {
                return 0;
            }
        }
    }
}
