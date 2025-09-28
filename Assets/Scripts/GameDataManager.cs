
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // 앱이 백그라운드로 전환될 때 (일시 중지될 때)
            // 이 곳에서 게임 데이터를 저장하는 로직을 호출합니다.
            Debug.Log("Application is pausing, saving data...");
            SaveGameData();
        }
    }

    private void OnApplicationQuit()
    {
        // 앱이 정상적으로 종료될 때
        // 만약을 위해 여기서도 저장 로직을 호출해 줄 수 있습니다.
        Debug.Log("Application is quitting, saving data...");
        SaveGameData();
    }

    public void SaveGameData()
    {
        // TODO: 이 곳에 실제 게임 데이터를 저장하는 코드를 구현하세요.
        // 예시: PlayerPrefs를 사용한 데이터 저장
        // PlayerPrefs.SetInt("PlayerScore", 12345);
        // PlayerPrefs.SetString("PlayerName", "Gemini");
        // PlayerPrefs.Save();

        Debug.Log("Game data saved!");
    }

    public void LoadGameData()
    {
        // TODO: 게임 시작 시 데이터를 불러오는 코드를 구현하세요.
        // 예시: PlayerPrefs를 사용한 데이터 불러오기
        // int playerScore = PlayerPrefs.GetInt("PlayerScore", 0);
        // string playerName = PlayerPrefs.GetString("PlayerName", "Default");
    }
}
