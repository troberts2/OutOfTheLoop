using System.Collections.Generic;
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
    public bool isTester = true;
}

[System.Serializable]
public class SaveFile
{
    public SettingsData settings = new SettingsData();
    public HighScoreData highscoreData = new HighScoreData();
    public SavedCosmeticData cosmeticData = new SavedCosmeticData();
    public TesterFlag testerFlag = new TesterFlag();
}

public enum MovementType
{
    FingerFollow,
    JoyStick,
    Tilt
}
