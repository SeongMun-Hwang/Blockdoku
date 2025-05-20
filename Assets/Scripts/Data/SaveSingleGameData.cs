using System.Collections.Generic;
using UnityEngine;

public static class SavePaths
{
    public static readonly string BlockDataPath = Application.persistentDataPath + "/blockData.json";
    public static string BoardDataPath = Application.persistentDataPath + "/save.json";
    public static string SettingDataPath = Application.persistentDataPath + "/setting.json";
}

[System.Serializable]
public class BlockSaveData
{
    public string prefabName;
    public int spawnIndex;
    public int rotationStep;
}
[System.Serializable]
public class BlockSaveDatas
{
    public List<BlockSaveData> blocks;
}
[System.Serializable]
public class SaveData
{
    public bool[] cubeFilledStates = new bool[81];
    public int score;
    public int combo;
}
[System.Serializable]
public class  AudioData
{
    public bool bgmMute;
    public bool sfxMute;
}
public class SaveSingleGameData : MonoBehaviour
{

}
