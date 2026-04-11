using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdManager : MonoBehaviour
{
    [SerializeField] private bool _enableAds = true;
    public bool EnableAds
    {
        get => _enableAds;
        set
        {
            _enableAds = value;
            if (!_enableAds) HideBanner();
            else ShowBanner();
        }
    }
    [SerializeField] private string _adUnitId;
    [SerializeField] private string _bannerAdUnitId;
    private InterstitialAd _interstitialAd;
    private BannerView _bannerView;

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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        AdEventBus.OnGamePlayEnded -= HandleGamePlayEnded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RecreateBannerCoroutine());
    }
    private IEnumerator RecreateBannerCoroutine()
    {
        yield return null; // UI Layout 기다림

        RecreateBanner();
    }
    private void RecreateBanner()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
        }

        if (!_enableAds) return;

        Debug.Log("Recreating banner...");

        AdSize adaptiveSize = AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        _bannerView = new BannerView(_bannerAdUnitId, adaptiveSize, AdPosition.Bottom);

        var adRequest = new AdRequest();
        _bannerView.LoadAd(adRequest);
    }
    private IEnumerator ReShowBannerCoroutine()
    {
        // UI 레이아웃이 확정될 때까지 한 프레임 대기
        yield return null;

        if (_bannerView != null && _enableAds)
        {
            Debug.Log("Re-showing banner after scene load with delay");
            _bannerView.Hide();
            _bannerView.Show();
        }
    }

    public void Start()
    {
        if (!_enableAds) return;

        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            LoadInterstitialAd();
            LoadBannerAd(); // ✅ 여기서는 “생성만”
        });
    }

    public void LoadBannerAd()
    {
        RecreateBanner();
    }

    public void SetBannerPosition(RectTransform placeholder)
    {
        if (_bannerView == null || placeholder == null || !_enableAds) return;

        Vector3[] corners = new Vector3[4];
        placeholder.GetWorldCorners(corners);

        float screenHeight = Screen.height;
        Vector2 screenPos = corners[1];

        int x = Mathf.RoundToInt(screenPos.x);
        int y = Mathf.RoundToInt(screenHeight - screenPos.y);

        _bannerView.SetPosition(x, y);
    }

    public void HideBanner()
    {
        if (_bannerView != null)
        {
            _bannerView.Hide();
            Debug.Log("Banner ad hidden.");
        }
    }

    public void ShowBanner()
    {
        if (_bannerView != null && _enableAds)
        {
            _bannerView.Show();
            Debug.Log("Banner ad shown.");
        }
    }

    // 좌표 변환을 돕는 유틸리티 클래스 (내부 정의 혹은 외부 사용 가능)
    private static class RuntimeCanvasUtils
    {
        public static Vector2 WorldToScreenPoint(Vector3 worldPoint)
        {
            return worldPoint; // WorldCorners는 이미 스크린 좌표(Overlay 기준)와 일치함
        }
    }

    private void HandleGamePlayEnded(MinigameType gameType, Action onComplete)
    {
        if (!_playCounts.ContainsKey(gameType))
        {
            _playCounts[gameType] = 0;
        }

        _playCounts[gameType]++;
        Debug.Log($"[AdManager] GamePlayEnded for {gameType}. Current count: {_playCounts[gameType]}");

        // If ad is null and ads are enabled, try to load it now
        if (_enableAds && _interstitialAd == null)
        {
            LoadInterstitialAd();
        }

        bool shouldShowAd = false;

        switch (gameType)
        {
            case MinigameType.Blockdoku:
            case MinigameType._2048:
                shouldShowAd = true;
                break;
            case MinigameType.TenSum:
                if (_playCounts[gameType] >= 3)
                {
                    shouldShowAd = true;
                }
                break;
            case MinigameType.MineSweeper:
                if (_playCounts[gameType] >= 2)
                {
                    shouldShowAd = true;
                }
                break;
        }

        if (_enableAds && shouldShowAd && _interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log($"[AdManager] Condition met. Showing ad for {gameType}. Resetting count.");
            _playCounts[gameType] = 0;
            _onAdClosedCallback = onComplete;
            ShowInterstitialAd();
        }
        else
        {
            if (shouldShowAd)
            {
                if (!_enableAds) Debug.Log("[AdManager] Ads are disabled.");
                else if (_interstitialAd == null) Debug.Log("[AdManager] Interstitial ad is null (still loading?).");
                else if (!_interstitialAd.CanShowAd()) Debug.Log("[AdManager] Interstitial ad is not ready yet.");
            }
            else
            {
                Debug.Log($"[AdManager] shouldShowAd is false for {gameType} (count: {_playCounts[gameType]})");
            }
            onComplete?.Invoke();
        }
    }

    private int _retryAttempt = 0;

    public void LoadInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        if (!_enableAds) return;

        Debug.Log("[AdManager] Loading Interstitial Ad...");
        var adRequest = new AdRequest();

        InterstitialAd.Load(_adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    _retryAttempt++;
                    double retryDelay = Math.Pow(2, Math.Min(6, _retryAttempt)); // 2, 4, 8, 16, 32, 64 seconds
                    Debug.LogError($"[AdManager] Interstitial ad failed to load: {error}. Retrying in {retryDelay}s...");
                    StartCoroutine(RetryLoadInterstitialAd(retryDelay));
                    return;
                }

                Debug.Log("[AdManager] Interstitial ad loaded successfully.");
                _interstitialAd = ad;
                _retryAttempt = 0; // Reset retry count on success

                // 광고가 닫혔을 때
                _interstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("[AdManager] Interstitial ad closed by user.");
                    ExecuteOnAdClosed();
                };

                // 광고 표시 실패 시
                _interstitialAd.OnAdFullScreenContentFailed += (AdError err) =>
                {
                    Debug.LogError("[AdManager] Interstitial ad failed to show: " + err);
                    ExecuteOnAdClosed();
                };
            });
    }

    private IEnumerator RetryLoadInterstitialAd(double delay)
    {
        yield return new WaitForSeconds((float)delay);
        if (_interstitialAd == null && _enableAds)
        {
            LoadInterstitialAd();
        }
    }

    private void ExecuteOnAdClosed()
    {
        Debug.Log("[AdManager] ExecuteOnAdClosed called.");
        // 콜백 실행 (씬 전환 등)
        Action callback = _onAdClosedCallback;
        _onAdClosedCallback = null;
        
        if (callback != null)
        {
            Debug.Log("[AdManager] Invoking callback (Scene Load).");
            callback.Invoke();
        }

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
