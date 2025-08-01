using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    private const string saveFileName = "save.json";
    string saveDirectory;
    string fullPath;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }

        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        fullPath = Path.Combine(saveDirectory, saveFileName);

        // Ensure /Saves/ folder exists
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
    }

    public void SaveSettings(float master, float music, float sfx, int index, bool fullscreen)
    {
        SaveFile saveFile = LoadGame();

        //populate settings
        saveFile.settings.masterVolume = master;
        saveFile.settings.musicVolume = music;
        saveFile.settings.sfxVolume = sfx;
        saveFile.settings.resolutionIndex = index;
        saveFile.settings.isFullscreen = fullscreen;

        //actually write and save in json
        string json = JsonUtility.ToJson(saveFile, true); // 'true' for pretty print (optional)
        File.WriteAllText(fullPath, json);
        Debug.Log("saved to " +  fullPath);
    }

    public SaveFile LoadGame()
    {
        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            return JsonUtility.FromJson<SaveFile>(json);
        }

        return new SaveFile(); // default data
    }
}
