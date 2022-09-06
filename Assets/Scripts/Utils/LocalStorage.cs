using LitJson;
using UnityEngine;

public static class LocalStorage
{
    public static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetString(key, value.ToString());
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        string value = PlayerPrefs.GetString(key);
        if (bool.TryParse(value, out bool result))
        {
            return result;
        }
        return defaultValue;
    }

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public static int GetInt(string key, int defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    public static float GetFloat(string key, float defaultValue)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public static string GetString(string key, string defaultValue = "")
    {
        if (PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetString(key);
        }

        return defaultValue;
    }

    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    public static void SetJsonData(string key, JsonData data)
    {
        var value = JsonMapper.ToJson(data);
        SetString(key, value);
    }

    public static JsonData GetJsonData(string key)
    {
        var str = GetString(key);
        return JsonMapper.ToObject(str);
    }

    public static void SetObject(string key, object value)
    {
        LogUtils.W("LocalStorage.SetObject 未实现");
    }

    public static object GetObject(string key)
    {
        LogUtils.W("LocalStorage.GetObject 未实现");
        return null;
    }
}