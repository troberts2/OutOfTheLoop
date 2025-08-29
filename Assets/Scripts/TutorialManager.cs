using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private RectTransform tutorialPanel;
    // Start is called before the first frame update
    void Start()
    {
        SaveFile save = SaveSystem.Instance.LoadGame();
        if(!save.settings.hasPlayedBefore)
        {
            //DisplayTutorial
            DisplayTutorial();
            SaveSystem.Instance.SaveHasPlayedBefore();
        }
    }

    public void DisplayTutorial()
    {
        tutorialPanel.anchoredPosition = new Vector2(0, 1000f);
        tutorialPanel.DOKill();
        tutorialPanel.DOAnchorPosY(0, 0.25f).SetEase(Ease.InBack);
    }

    public void HideTutorial()
    {
        tutorialPanel.anchoredPosition = new Vector2(0, 0f);
        tutorialPanel.DOKill();
        tutorialPanel.DOAnchorPosY(1000f, 0.25f).SetEase(Ease.OutBack);
    }
}
