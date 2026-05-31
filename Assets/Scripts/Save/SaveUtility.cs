using UnityEngine;
using System.IO;
using System;

namespace Havengard.Save
{
    /// <summary>
    /// Utility class for JSON serialization and file operations
    /// </summary>
    public static class SaveUtility
    {
        private static readonly string SaveDirectory = Application.persistentDataPath + "/Saves/";
        private const string SaveFileExtension = ".json";

        /// <summary>
        /// Get the full path for a save file
        /// </summary>
        public static string GetSaveFilePath(string saveFileName)
        {
            return SaveDirectory + saveFileName + SaveFileExtension;
        }

        /// <summary>
        /// Ensure save directory exists
        /// </summary>
        public static void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
                Debug.Log($"[SaveUtility] Created save directory: {SaveDirectory}");
            }
        }

        /// <summary>
        /// Save data to JSON file
        /// </summary>
        public static bool SaveToFile<T>(T data, string saveFileName) where T : class
        {
            try
            {
                EnsureSaveDirectoryExists();

                string json = JsonUtility.ToJson(data, prettyPrint: true);
                string filePath = GetSaveFilePath(saveFileName);

                File.WriteAllText(filePath, json);

                Debug.Log($"[SaveUtility] Game saved to: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveUtility] Failed to save game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load data from JSON file
        /// </summary>
        public static T LoadFromFile<T>(string saveFileName) where T : class, new()
        {
            try
            {
                string filePath = GetSaveFilePath(saveFileName);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[SaveUtility] Save file not found: {filePath}");
                    return null;
                }

                string json = File.ReadAllText(filePath);
                T data = JsonUtility.FromJson<T>(json);

                Debug.Log($"[SaveUtility] Game loaded from: {filePath}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveUtility] Failed to load game: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Check if a save file exists
        /// </summary>
        public static bool SaveFileExists(string saveFileName)
        {
            return File.Exists(GetSaveFilePath(saveFileName));
        }

        /// <summary>
        /// Delete a save file
        /// </summary>
        public static bool DeleteSaveFile(string saveFileName)
        {
            try
            {
                string filePath = GetSaveFilePath(saveFileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"[SaveUtility] Deleted save file: {filePath}");
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveUtility] Failed to delete save file: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get all save file names (without extension)
        /// </summary>
        public static string[] GetAllSaveFileNames()
        {
            try
            {
                EnsureSaveDirectoryExists();

                string[] files = Directory.GetFiles(SaveDirectory, "*" + SaveFileExtension);
                string[] fileNames = new string[files.Length];

                for (int i = 0; i < files.Length; i++)
                {
                    fileNames[i] = Path.GetFileNameWithoutExtension(files[i]);
                }

                return fileNames;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveUtility] Failed to get save file list: {e.Message}");
                return new string[0];
            }
        }
    }
}