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
            Destroy(gameObject);
        }

        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        fullPath = Path.Combine(saveDirectory, saveFileName);

        // Ensure /Saves/ folder exists
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
    }

    public void SaveSettings(float master, float music, float sfx, int index, bool fullscreen, Vector3 calibrationOff, MovementType savedMovementType)
    {
        SaveFile saveFile = LoadGame();

        //populate settings
        saveFile.settings.masterVolume = master;
        saveFile.settings.musicVolume = music;
        saveFile.settings.sfxVolume = sfx;
        saveFile.settings.resolutionIndex = index;
        saveFile.settings.isFullscreen = fullscreen;
        saveFile.settings.calibrationOffset = calibrationOff;
        saveFile.settings.currentMovementType = savedMovementType;

        //actually write and save in json
        string json = JsonUtility.ToJson(saveFile, true); // 'true' for pretty print (optional)
        File.WriteAllText(fullPath, json);
        Debug.Log("saved to " +  fullPath);
    }

    public void SaveHasPlayedBefore()
    {
        SaveFile save = LoadGame();

        save.settings.hasPlayedBefore = true;

        //actually write and save in json
        string json = JsonUtility.ToJson(save, true); // 'true' for pretty print (optional)
        File.WriteAllText(fullPath, json);
        Debug.Log("saved to " + fullPath);
    }

    public void SaveHighScore(int score)
    {
        SaveFile save = LoadGame();

        save.highscoreData.HighScore = score;
        //actually write and save in json
        string json = JsonUtility.ToJson(save, true); // 'true' for pretty print (optional)
        File.WriteAllText(fullPath, json);
        Debug.Log("saved to " + fullPath);
    }

    public void SaveCosmetics(Hat hat, Shirt shirt, Sprite trail)
    {
        SaveFile save = LoadGame();

        save.cosmeticData.savedHat = hat;
        save.cosmeticData.savedShirt = shirt;
        save.cosmeticData.savedTrail = trail;

        //actually write and save in json
        string json = JsonUtility.ToJson(save, true); // 'true' for pretty print (optional)
        File.WriteAllText(fullPath, json);
        Debug.Log("saved to " + fullPath);
    }

    public void SaveLocalUnlocks(Dictionary<string, bool> unlockedProducts)
    {
        SaveFile save = LoadGame();

        save.localUnlocks.savedUnlockedProducts.FromDictionary(unlockedProducts);

        //actually write and save in json
        string json = JsonUtility.ToJson(save, true); // 'true' for pretty print (optional)
        File.WriteAllText(fullPath, json);
        Debug.Log("saved to " + fullPath);
    }

    public Dictionary<string, bool> LoadUnlocks()
    {
        if (!File.Exists(fullPath)) return new Dictionary<string, bool>();

        string json = File.ReadAllText(fullPath);
        SaveFile data = JsonUtility.FromJson<SaveFile>(json);
        return data.localUnlocks.savedUnlockedProducts.ToDictionary();
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

    public void ClearAllSaveData()
    {
        SaveFile save = new SaveFile();

        //actually write and save in json
        string json = JsonUtility.ToJson(save, true); // 'true' for pretty print (optional)
        File.WriteAllText(fullPath, json);
        Debug.Log("saved to " + fullPath);
    }
}
