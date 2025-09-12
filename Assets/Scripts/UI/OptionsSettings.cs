using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionsSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resDropDown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown movementDropdown;

    [Header("Calibration")]
    [SerializeField] private RectTransform calibrationPanel;

    bool isFullscreen;
    int selectedResolution;
    List<Resolution> selectedResolutionList = new List<Resolution>();

    private void OnEnable()
    {
        PauseManager.OnOptionsClose += SaveSettings;
    }

    private void OnDisable()
    {
        PauseManager.OnOptionsClose -= SaveSettings;
    }

    // Start is called before the first frame update
    void Start()
    {
        isFullscreen = false;
        List<Resolution> resolutions = new List<Resolution>
        {
            CreateNewResolution(640, 360),
            CreateNewResolution(1280, 720),
            CreateNewResolution(1920, 1080)
        };

        List<string> resolutionStringList = new List<string>
        {
            "640 x 360",
            "1280 x 720",
            "1920 x 1080"
        };

        resDropDown.AddOptions(resolutionStringList);
        selectedResolutionList = resolutions;

        //audio stuff
        SaveFile save = SaveSystem.Instance.LoadGame();
        LoadMasterVolume(save);
        LoadMusicVolume(save);
        LoadSfxVolume(save);

        movementDropdown.value = (int)save.settings.currentMovementType;

        if (movementDropdown.value == 2)
        {
            OpenCalibrateScreen();
            Screen.orientation = OrientationUtils.GetCurrentOrientation();
        }
        else
        {
            // Re-enable autorotation
            Screen.orientation = ScreenOrientation.AutoRotation;

            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
        }

#if UNITY_WEBGL
        //set resolution to 1280x720 for webGL window. Player cannot fullscreen or change res
        selectedResolution = 1;
        //Screen.SetResolution(1280, 720, isFullscreen);
#endif

#if UNITY_STANDALONE
        //set saved fullscreen and resolution options
        SetSavedResolution(save);
#endif
    }

    private Resolution CreateNewResolution(int width, int height)
    {
        Resolution newRes = new Resolution();
        newRes.width = width;
        newRes.height = height;
        return newRes;
    }

    public void ChangeResolution()
    {
        selectedResolution = resDropDown.value;
        Screen.SetResolution(selectedResolutionList[selectedResolution].width, selectedResolutionList[selectedResolution].height, isFullscreen);
    }

    public void ChangeFullscreen()
    {
        isFullscreen = fullscreenToggle.isOn;
        if(isFullscreen)
        {
            //resDropDown.value = 2; //change res to 1920x1080 if changing to full screen
            selectedResolution = resDropDown.value;
        }
        else
        {
            selectedResolution = resDropDown.value;
        }
        
        Screen.SetResolution(selectedResolutionList[selectedResolution].width, selectedResolutionList[selectedResolution].height, isFullscreen);
    }

    public void ChangeMovementType()
    {
        GameManager.Instance.movementType = (MovementType)movementDropdown.value;
        if(movementDropdown.value == 2)
        {
            OpenCalibrateScreen();
            Screen.orientation = OrientationUtils.GetCurrentOrientation();
        }
        else
        {
            // Re-enable autorotation
            Screen.orientation = ScreenOrientation.AutoRotation;

            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
        }
    }

    private void OpenCalibrateScreen()
    {
        calibrationPanel.anchoredPosition = new Vector2(0, 1000);
        calibrationPanel.DOAnchorPosY(0, 0.2f).SetEase(Ease.InBack).SetUpdate(true);
    }

    private void CloseCalibrateScreen()
    {
        calibrationPanel.anchoredPosition = Vector2.zero;
        calibrationPanel.DOAnchorPosY(1000, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void Calibrate()
    {
        if (Accelerometer.current != null)
        {
            GameManager.Instance.calibrationOffset = Accelerometer.current.acceleration.ReadValue();
        }
        CloseCalibrateScreen();
    }

    public void SetSavedResolution(SaveFile save)
    {
        int resIndex = save.settings.resolutionIndex;
        Resolution savedRes = selectedResolutionList[resIndex];
        bool isFullscreenSaved = save.settings.isFullscreen;
        Screen.SetResolution(savedRes.width, savedRes.height, isFullscreenSaved);
        resDropDown.value = resIndex;
        fullscreenToggle.isOn = isFullscreenSaved;
    }

    private void SaveSettings()
    {
        SaveSystem.Instance.SaveSettings(masterSlider.value, 
            musicSlider.value, 
            sfxSlider.value, 
            selectedResolution, 
            isFullscreen, 
            GameManager.Instance.calibrationOffset,
            (MovementType)movementDropdown.value
            );
    }

    #region Sound Settings

    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    public void SetMasterVolume()
    {
        float volume = masterSlider.value;
        myMixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20);
    }
    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        myMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
    }
    public void SetSoundFXVolume()
    {
        float volume = sfxSlider.value;
        myMixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20);
    }
    private void LoadMusicVolume(SaveFile saveFile)
    {
        musicSlider.value = saveFile.settings.musicVolume;

        SetMusicVolume();
    }
    private void LoadSfxVolume(SaveFile saveFile)
    {
        sfxSlider.value = saveFile.settings.sfxVolume;

        SetSoundFXVolume();
    }
    private void LoadMasterVolume(SaveFile saveFile)
    {
        masterSlider.value = saveFile.settings.masterVolume;

        SetMasterVolume();
    }

    #endregion}
}
