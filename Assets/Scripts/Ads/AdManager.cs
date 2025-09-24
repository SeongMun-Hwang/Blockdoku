using GoogleMobileAds.Api;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    [SerializeField] private string _adUnitId;
    private InterstitialAd _interstitialAd;

    public static AdManager Instance { get; private set; }

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

    public void Start()
    {
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            LoadInterstitialAd();
        });
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
        }
    }
}
