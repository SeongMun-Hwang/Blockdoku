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
            DontDestroyOnLoad(gameObject);
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
        // 광고 SDK 이벤트를 유니티 메인 스레드에서 실행하도록 보장 (지연 감소 핵심)
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // 초기화 후 첫 광고 로드
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
                shouldShowAd = true;
                break;
            case MinigameType.TenSum:
            case MinigameType.MineSweeper:
                if (_playCounts[gameType] >= 2)
                {
                    shouldShowAd = true;
                    _playCounts[gameType] = 0;
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
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");
        var adRequest = new AdRequest();

        InterstitialAd.Load(_adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Interstitial ad failed to load: " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded.");
                _interstitialAd = ad;

                // 광고가 닫혔을 때
                _interstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("Interstitial ad closed.");
                    ExecuteOnAdClosed();
                };

                // 광고 표시 실패 시
                _interstitialAd.OnAdFullScreenContentFailed += (AdError err) =>
                {
                    Debug.LogError("Interstitial ad failed to show: " + err);
                    ExecuteOnAdClosed();
                };
            });
    }

    private void ExecuteOnAdClosed()
    {
        // 콜백 실행 (게임오버 패널 표시 등)
        Action callback = _onAdClosedCallback;
        _onAdClosedCallback = null;
        callback?.Invoke();

        // 다음 광고 미리 로드
        LoadInterstitialAd();
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
            ExecuteOnAdClosed();
        }
    }
}
