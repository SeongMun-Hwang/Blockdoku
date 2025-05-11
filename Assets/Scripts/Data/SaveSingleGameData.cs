using System.Collections.Generic;
using UnityEngine;

public static class SavePaths
{
    public static readonly string BlockDataPath = Application.persistentDataPath + "/blockData.json";
    public static string BoardDataPath = Application.persistentDataPath + "/save.json";
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
public class SaveSingleGameData : MonoBehaviour
{

}
