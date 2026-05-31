/*ARCHIVE
 * using UnityEngine;
using Havengard.Save;

public class SaveLoadTestARCHIVE : MonoBehaviour
{
    private void Update()
    {
        // Press F5 to save
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("=== SAVING GAME (F5) ===");
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }
            else
            {
                Debug.LogError("SaveManager.Instance is NULL!");
            }
        }

        // Press F9 to load
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log("=== LOADING GAME (F9) ===");
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.LoadGame();
            }
            else
            {
                Debug.LogError("SaveManager.Instance is NULL!");
            }
        }

        // Press F10 to check save files
        if (Input.GetKeyDown(KeyCode.F10))
        {
            Debug.Log("=== CHECKING SAVE FILES (F10) ===");
            if (SaveManager.Instance != null)
            {
                bool exists = SaveManager.Instance.SaveExists();
                Debug.Log($"Default save file exists: {exists}");

                string[] saves = SaveManager.Instance.GetAllSaveFiles();
                Debug.Log($"Total save files: {saves.Length}");
                foreach (string save in saves)
                {
                    Debug.Log($"  - {save}");
                }
                Debug.Log($"Save location: {Application.persistentDataPath}/Saves/");
            }
            else
            {
                Debug.LogError("SaveManager.Instance is NULL!");
            }
        }
    }
}*/