using UnityEngine;
using Unity.Services.LevelPlay;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;
    [SerializeField] private string myAppKey = "233467b85";
    [SerializeField] private string adUnitBanner = "266jptcm2tkaaxfr";
    [SerializeField] private string adUnitInterstitial = "bwel4wx7fs4g0vko";
    [SerializeField] private string adUnitRewarded = "20gk4wdr9nlgtw8m";
    public LevelPlayBannerAd bannerAd;
    public LevelPlayInterstitialAd interstitialAd;
    public LevelPlayRewardedAd rewardedVideoAd;

    public bool isAdsEnabled = true;
    public bool hasWatchedAdThisRun = false;

    public static event Action OnPlayerContinueReward;

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
    }

    private void OnEnable()
    {
        //LevelPlay.OnInitSuccess += SdkInitializationSuccessEvent;
        //LevelPlay.OnInitFailed += SdkInitializationFailedEvent;
        GameManager.OnGameReset += OnGameReset;
    }

    private void OnDisable()
    {
        //LevelPlay.OnInitSuccess -= SdkInitializationSuccessEvent;
        //LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;
        GameManager.OnGameReset -= OnGameReset;

        bannerAd?.DestroyAd();
        interstitialAd?.DestroyAd();
    }

    public void DisableAds()
    {
        isAdsEnabled = false;
    }

    private void Start()
    {
        //LevelPlay.ValidateIntegration();
        // Optional: enable verbose logging
        //LevelPlay.SetAdaptersDebug(true);
        //LevelPlay.SetMetaData("is_test_suite", "enable");

        //LevelPlay.SetMetaData("is_deviceid_optout", "true");
        //LevelPlay.SetMetaData("is_child_directed", "true");
        //LevelPlay.SetMetaData("Google_Family_Self_Certified_SDKS", "true");

        //LevelPlay.Init(myAppKey);
    }

    private void OnGameReset()
    {
        hasWatchedAdThisRun = false;
    }

    #region banner

    public void LoadBanner()
    {
        bannerAd.LoadAd();
    }

    public void ShowBanner()
    {
        bannerAd?.ShowAd();
    }

    public void DestroyBanner()
    {
        bannerAd?.DestroyAd();
    }

    #endregion

    #region rewarded

    public void LoadRewarded()
    {
        rewardedVideoAd.LoadAd();
    }

    public void ShowRewarded()
    {
        rewardedVideoAd?.ShowAd();
        hasWatchedAdThisRun = true;
    }

    public void DestroyRewarded()
    {
        rewardedVideoAd?.DestroyAd();
    }

    private void OnPlayerContinueRewardHandler()
    {
        OnPlayerContinueReward?.Invoke();
        //normal time
        Time.timeScale = 1.0f;
    }

    #endregion

    #region Interstitial

    public void LoadInterstitial()
    {
        interstitialAd.LoadAd();
    }

    public void ShowInterstitial()
    {
        if (!isAdsEnabled) return; //skip ads for paid users

        interstitialAd?.ShowAd();
    }

    public void DestroyInterstitial()
    {
        interstitialAd?.DestroyAd();
    }

    #endregion

    #region SDKINIT

    private void SdkInitializationSuccessEvent(LevelPlayConfiguration config)
    {
        Debug.Log("Initialized levelPlay success");
        //EnableAds();
        //LoadInterstitial();
        //LoadRewarded();

        //LevelPlay.LaunchTestSuite();
    }

    private void SdkInitializationFailedEvent(LevelPlayInitError error)
    {
        Debug.Log("Initialized levelPlay error");
    }

    void EnableAds()
    {
        // Register to ImpressionDataReadyEvent
        LevelPlay.OnImpressionDataReady += ImpressionDataReadyEvent;

        // Create Rewarded Video object
        //rewardedVideoAd = new LevelPlayRewardedAd(adUnitRewarded);

        // Register to Rewarded Video events
        rewardedVideoAd.OnAdLoaded += RewardedVideoOnLoadedEvent;
        rewardedVideoAd.OnAdLoadFailed += RewardedVideoOnAdLoadFailedEvent;
        rewardedVideoAd.OnAdDisplayed += RewardedVideoOnAdDisplayedEvent;
        //rewardedVideoAd.OnAdDisplayFailed += RewardedVideoOnAdDisplayedFailedEvent;
        rewardedVideoAd.OnAdRewarded += RewardedVideoOnAdRewardedEvent;
        rewardedVideoAd.OnAdClicked += RewardedVideoOnAdClickedEvent;
        rewardedVideoAd.OnAdClosed += RewardedVideoOnAdClosedEvent;
        rewardedVideoAd.OnAdInfoChanged += RewardedVideoOnAdInfoChangedEvent;

        // Create Banner object
       // bannerAd = new LevelPlayBannerAd(adUnitBanner);

        // Register to Banner events
        bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
        bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
        bannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
        //bannerAd.OnAdDisplayFailed += BannerOnAdDisplayFailedEvent;
        bannerAd.OnAdClicked += BannerOnAdClickedEvent;
        bannerAd.OnAdCollapsed += BannerOnAdCollapsedEvent;
        bannerAd.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
        bannerAd.OnAdExpanded += BannerOnAdExpandedEvent;

        // Create Interstitial object
       // interstitialAd = new LevelPlayInterstitialAd(adUnitInterstitial);

        // Register to Interstitial events
        interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
        interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
        interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
        //interstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
        interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
        interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
        interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;
    }

    #endregion

    #region AdInfo Rewarded Video
    void RewardedVideoOnLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnLoadedEvent With AdInfo: {adInfo}");
    }

    void RewardedVideoOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdLoadFailedEvent With Error: {error}");
    }

    void RewardedVideoOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdDisplayedEvent With AdInfo: {adInfo}");
    }
#pragma warning disable 0618
/*    void RewardedVideoOnAdDisplayedFailedEvent(LevelPlayAdDisplayInfoError error)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdDisplayedFailedEvent With Error: {error}");
    }*/
#pragma warning restore 0618
    void RewardedVideoOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        OnPlayerContinueRewardHandler();
    }

    void RewardedVideoOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdClickedEvent With AdInfo: {adInfo}");
    }

    void RewardedVideoOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        //refresh after close automatically
        LoadRewarded();
    }

    void RewardedVideoOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdInfoChangedEvent With AdInfo {adInfo}");
    }

    #endregion
    #region AdInfo Interstitial

    void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdLoadedEvent With AdInfo: {adInfo}");
    }

    void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdLoadFailedEvent With Error: {error}");
    }

    void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdDisplayedEvent With AdInfo: {adInfo}");
    }
/*#pragma warning disable 0618
    void InterstitialOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError infoError)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdDisplayFailedEvent With InfoError: {infoError}");
    }*/
#pragma warning restore 0618
    void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdClickedEvent With AdInfo: {adInfo}");
    }

    void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        //refresh after close automatically
        LoadInterstitial();
    }

    void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdInfoChangedEvent With AdInfo: {adInfo}");
    }

    #endregion

    #region Banner AdInfo

    void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdLoadedEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdLoadFailedEvent With Error: {error}");
    }

    void BannerOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdClickedEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdDisplayedEvent With AdInfo: {adInfo}");
    }
#pragma warning disable 0618
/*    void BannerOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError adInfoError)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdDisplayFailedEvent With AdInfoError: {adInfoError}");
    }*/
#pragma warning restore 0618
    void BannerOnAdCollapsedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdCollapsedEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdLeftApplicationEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdExpandedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdExpandedEvent With AdInfo: {adInfo}");
    }

    #endregion

    #region ImpressionSuccess callback handler

    void ImpressionDataReadyEvent(LevelPlayImpressionData impressionData)
    {
        Debug.Log($"[LevelPlaySample] Received ImpressionDataReadyEvent ToString(): {impressionData}");
        Debug.Log($"[LevelPlaySample] Received ImpressionDataReadyEvent allData: {impressionData.AllData}");
    }

    #endregion
}
