using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    [SerializeField] private string _adUnitId;
    private InterstitialAd _interstitialAd;

    public static AdManager Instance { get; private set; }

    private Action _onAdClosedCallback;
    private Dictionary<MinigameType, int> _playCounts = new Dictionary<MinigameType, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 선택 사항: 씬 이동 간 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        AdEventBus.OnGamePlayEnded += HandleGamePlayEnded;
    }

    private void OnDisable()
    {
        AdEventBus.OnGamePlayEnded -= HandleGamePlayEnded;
    }

    public void Start()
    {
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            LoadInterstitialAd();
        });
    }

    private void HandleGamePlayEnded(MinigameType gameType, Action onComplete)
    {
        if (!_playCounts.ContainsKey(gameType))
        {
            _playCounts[gameType] = 0;
        }

        _playCounts[gameType]++;

        bool shouldShowAd = false;

        switch (gameType)
        {
            case MinigameType.Blockdoku:
                // Blockdoku: Show ad every time game ends
                shouldShowAd = true;
                break;
            case MinigameType.TenSum:
            case MinigameType.MineSweeper:
                // 10SUM & MineSweeper: Show ad every 2nd time (including restarts)
                if (_playCounts[gameType] >= 2)
                {
                    shouldShowAd = true;
                    _playCounts[gameType] = 0; // Reset count
                }
                break;
        }

        if (shouldShowAd && _interstitialAd != null && _interstitialAd.CanShowAd())
        {
            _onAdClosedCallback = onComplete;
            ShowInterstitialAd();
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    public void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        InterstitialAd.Load(_adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _interstitialAd = ad;

                //광고가 닫혔을 때 다음 광고 로드
                _interstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("Interstitial ad closed. Loading next ad...");
                    
                    Action callback = _onAdClosedCallback;
                    _onAdClosedCallback = null;
                    callback?.Invoke();

                    LoadInterstitialAd(); // 다음 광고 미리 로드
                };

                // (선택) 광고가 열렸을 때
                _interstitialAd.OnAdFullScreenContentOpened += () =>
                {
                    Debug.Log("Interstitial ad opened.");
                };

                // (선택) 광고 표시 실패
                _interstitialAd.OnAdFullScreenContentFailed += (AdError err) =>
                {
                    Debug.LogError("Interstitial ad failed to show with error: " + err);
                    
                    Action callback = _onAdClosedCallback;
                    _onAdClosedCallback = null;
                    callback?.Invoke();
                    
                    LoadInterstitialAd(); // 실패해도 로드 시도
                };
            });
    }

    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
            // 만약 광고를 보여주려 했는데 준비가 안된 경우라면 바로 콜백 실행
            Action callback = _onAdClosedCallback;
            _onAdClosedCallback = null;
            callback?.Invoke();
        }
    }
}
