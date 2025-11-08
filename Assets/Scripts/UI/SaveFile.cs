using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SettingsData
{
    public float masterVolume = .7f;
    public float musicVolume = .7f;
    public float sfxVolume = .7f;
    public int resolutionIndex = 0;
    public bool isFullscreen = false;
    public Vector3 calibrationOffset = Vector3.zero;
    public MovementType currentMovementType = MovementType.FingerFollow;
    public bool hasPlayedBefore = false;
}

[System.Serializable]
public class HighScoreData
{
    public int HighScore = 0;
}

[System.Serializable]
public class SavedCosmeticData
{
    public Hat savedHat = null;
    public Shirt savedShirt = null;
    public Sprite savedTrail = null;
}

[System.Serializable]
public class TesterFlag
{
    //changed for non test versions of game
    public bool isTester = false;
}

[System.Serializable]
public class LocalUnlocks
{
    // Keeps runtime unlocked state in memory; mirror it to your JSON save system.
    // Key = productId, Value = unlocked (true/false)
    public SerializableDictionary savedUnlockedProducts = new();
}

[System.Serializable]
public class SaveFile
{
    public SettingsData settings = new SettingsData();
    public HighScoreData highscoreData = new HighScoreData();
    public SavedCosmeticData cosmeticData = new SavedCosmeticData();
    public TesterFlag testerFlag = new TesterFlag();
    public LocalUnlocks localUnlocks = new LocalUnlocks();
}

public enum MovementType
{
    FingerFollow,
    JoyStick,
    Tilt
}

[System.Serializable]
public class SerializableDictionary
{
    public List<string> keys = new();
    public List<bool> values = new();

    public Dictionary<string, bool> ToDictionary()
    {
        var dict = new Dictionary<string, bool>();
        for (int i = 0; i < keys.Count; i++)
            dict[keys[i]] = values[i];
        return dict;
    }

    public void FromDictionary(Dictionary<string, bool> dict)
    {
        keys = dict.Keys.ToList();
        values = dict.Values.ToList();
    }
}
