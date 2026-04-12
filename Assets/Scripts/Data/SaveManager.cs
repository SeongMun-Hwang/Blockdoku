using System.IO;
using UnityEngine;

public static class SaveManager
{
    public static void SaveData<T>(string fileName, T data)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
        Debug.Log($"Data saved to {path}");
    }

    public static T LoadData<T>(string fileName) where T : new()
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        return new T();
    }

    public static bool Exists(string fileName)
    {
        return File.Exists(Path.Combine(Application.persistentDataPath, fileName));
    }

    public static void Delete(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
