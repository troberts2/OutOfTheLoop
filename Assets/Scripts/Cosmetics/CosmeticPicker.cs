using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CosmeticPicker : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private Color panelActiveColor;
    [SerializeField] private Color panelInactiveColor;
    [SerializeField] private CosmeticManager cosmeticManager;

    [Header("Hat Panel")]
    [SerializeField] private GameObject hatPanel;
    [SerializeField] private Button hatPanelButton;
    [SerializeField] private Image[] allHatButtonImages;
    [SerializeField] private Hat[] allHats;

    [Header("Shirt Panel")]
    [SerializeField] private GameObject shirtPanel;
    [SerializeField] private Button shirtPanelButton;
    [SerializeField] private Image[] allShirtButtonImages;
    [SerializeField] private Shirt[] allShirts;

    [Header("Trail Panel")]
    [SerializeField] private GameObject trailPanel;
    [SerializeField] private Button trailPanelButton;
    [SerializeField] private Image[] allTrailButtonImages;
    [SerializeField] private Sprite[] allTrails;

    [Header("Player UI Move")]
    [SerializeField] private RectTransform uiElement;   // Your UI element (anchored)
    [SerializeField] private Camera worldCamera;        // The camera rendering the world
    [SerializeField] private Transform target;          // The world object to move

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode load)
    {
        SelectSavedHat();
        SelectSavedShirt();
        SelectSavedTrail();

        // Step 1: Get the UI element's screen position
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, uiElement.position);

        // Step 2: Convert screen position to world position in 2D
        Vector3 worldPos = worldCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, worldCamera.nearClipPlane));

        // Step 3: Set Z to match your 2D world plane (e.g., z = 0)
        worldPos.z = 0f;

        // Step 4: Assign to target
        target.position = worldPos;
    }

    public void HatPanelButton()
    {
        //set other two panels inactive, set their buttons inactive color
        shirtPanel.SetActive(false);
        shirtPanelButton.GetComponent<Image>().color = panelInactiveColor;

        trailPanel.SetActive(false);
        trailPanelButton.GetComponent<Image>().color = panelInactiveColor;

        //active the hat panel and set button color to active color
        hatPanel.SetActive(true);
        hatPanelButton.GetComponent<Image>().color = panelActiveColor;
    }

    public void ShirtPanelButton()
    {
        //set other two panels inactive, set their buttons inactive color
        hatPanel.SetActive(false);
        hatPanelButton.GetComponent<Image>().color = panelInactiveColor;

        trailPanel.SetActive(false);
        trailPanelButton.GetComponent<Image>().color = panelInactiveColor;

        //active the shirt panel and set button color to active color
        shirtPanel.SetActive(true);
        shirtPanelButton.GetComponent<Image>().color = panelActiveColor;
    }

    public void TrailPanelButton()
    {
        //set other two panels inactive, set their buttons inactive color
        shirtPanel.SetActive(false);
        shirtPanelButton.GetComponent<Image>().color = panelInactiveColor;

        hatPanel.SetActive(false);
        hatPanelButton.GetComponent<Image>().color = panelInactiveColor;

        //active the trail panel and set button color to active color
        trailPanel.SetActive(true);
        trailPanelButton.GetComponent<Image>().color = panelActiveColor;
    }

    #region Hats

    public void DeselectAllHats()
    {
        foreach(Image hatButton in allHatButtonImages)
        {
            hatButton.color = panelInactiveColor;
        }
    }

    public void SelectSavedHat()
    {
        if (GameManager.Instance.currentHat == null)
        {
            allHatButtonImages[0].color = panelActiveColor;
            return;
        }

        switch (GameManager.Instance.currentHat.hatName)
        {
            case "Top Hat":
                //set top hat button color as selected
                allHatButtonImages[1].color = panelActiveColor;
                break;
            case "Banana Hat":
                //set top hat button color as selected
                allHatButtonImages[2].color = panelActiveColor;
                break;
            default:
                //no hat selected leave them all not selected
                //except for no select button
                allHatButtonImages[0].color = panelActiveColor;
                break;
        }
    }

    public void NoHat()
    {
        //set the buttons inactive
        DeselectAllHats();
        allHatButtonImages[0].color = panelActiveColor;
        GameManager.Instance.currentHat = null; //no hat
        cosmeticManager.currentHat = null;
    }

    public void TopHat()
    {
        DeselectAllHats();
        //set top hat button color as selected
        allHatButtonImages[1].color = panelActiveColor;
        GameManager.Instance.currentHat = allHats[0]; // top hat
        cosmeticManager.currentHat = allHats[0];
    }

    public void BananaHat()
    {
        DeselectAllHats();
        //set top hat button color as selected
        allHatButtonImages[2].color = panelActiveColor;
        GameManager.Instance.currentHat = allHats[1]; // top hat
        cosmeticManager.currentHat = allHats[1];
    }

    #endregion

    #region Shirts

    public void DeselectAllShirts()
    {
        foreach (Image shirtButton in allShirtButtonImages)
        {
            shirtButton.color = panelInactiveColor;
        }
    }

    public void SelectSavedShirt()
    {
        if (GameManager.Instance.currentShirt == null)
        {
            allShirtButtonImages[0].color = panelActiveColor;
            return;
        }

        switch (GameManager.Instance.currentShirt.shirtName)
        {
            case "Suit":
                //set top hat button color as selected
                allShirtButtonImages[1].color = panelActiveColor;
                break;
            default:
                //no hat selected leave them all not selected
                //except for no select button
                allShirtButtonImages[0].color = panelActiveColor;
                break;
        }
    }

    public void NoShirt()
    {
        //set the buttons inactive
        DeselectAllShirts();
        allShirtButtonImages[0].color = panelActiveColor;
        GameManager.Instance.currentShirt = null; // no shirt
        cosmeticManager.currentShirt = null;
    }

    public void Suit()
    {
        DeselectAllShirts();
        //set top hat button color as selected
        allShirtButtonImages[1].color = panelActiveColor;
        GameManager.Instance.currentShirt = allShirts[0]; // suit
        cosmeticManager.currentShirt = allShirts[0];
    }

    #endregion

    #region Trails

    public void DeselectAllTrails()
    {
        foreach (Image trailButton in allTrailButtonImages)
        {
            trailButton.color = panelInactiveColor;
        }
    }

    public void SelectSavedTrail()
    {
        if (GameManager.Instance.currentTrail == null)
        {
            allTrailButtonImages[0].color = panelActiveColor;
            return;
        }

        if (GameManager.Instance.currentTrail == allTrails[0])
        {
            // set top hat button color as selected
            allTrailButtonImages[1].color = panelActiveColor;
        }
        else
        {
            allTrailButtonImages[0].color = panelActiveColor;
        }
    }

    public void NoTrail()
    {
        //set the buttons inactive
        DeselectAllTrails();
        allTrailButtonImages[0].color = panelActiveColor;
        GameManager.Instance.currentTrail = null; // no trail
        //
    }

    public void BananaTrail()
    {
        DeselectAllTrails();
        allTrailButtonImages[1].color = panelActiveColor;
        GameManager.Instance.currentTrail = allTrails[0];
        cosmeticManager.currentTrail = allTrails[0];
    }

    #endregion

    public void BackToMainMenu()
    {
        //save the current cosmetics
        SaveSystem.Instance.SaveCosmetics(GameManager.Instance.currentHat, GameManager.Instance.currentShirt, GameManager.Instance.currentTrail);
        SceneManager.LoadScene("MainMenu");
    }
}
