using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Excel;
using System.Data;
using System.Text.RegularExpressions;

class RenamFishID
{
    const string kKeyOfAutoRefresh = "kAutoRefresh";
    static bool mEnding;

    static void SetAutoRefresh(bool b)
    {
        if (EditorPrefs.HasKey(kKeyOfAutoRefresh))
        {
            EditorPrefs.SetBool(kKeyOfAutoRefresh, b);
        }
    }

    #region 替换鱼资源id
    //[MenuItem("Tools/Art/替换鱼的id")]
    public static void DoIt()
    {
        SetAutoRefresh(false);
        string filePath = "Assets/GameData/Excel~/微乐4.0新旧版鱼ID.xlsx";
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet result = excelReader.AsDataSet();
        var table = result.Tables[0];
        int columnNum = table.Columns.Count;
        int rowNum = table.Rows.Count;
        int count = 0;
        for (int i = 1; i < rowNum; i++)
        {
            if (mEnding)
            {
                break;
            }
            string oldIDStr = table.Rows[i][0].ToString();
            string newIDStr = table.Rows[i][1].ToString();
            string fishname = table.Rows[i][2].ToString();
            if (string.IsNullOrEmpty(oldIDStr) || string.IsNullOrEmpty(newIDStr))
            {
                continue;
            }
            int oldid = 0;
            int newid = 0;
            int.TryParse(oldIDStr, out oldid);
            int.TryParse(newIDStr, out newid);
            if (oldid == 0 || newid == 0)
            {
                continue;
            }
            if (oldid == 100000306)
            {
                continue;
            }
            if (oldid != newid)
            {
                count++;
                Debug.Log($"替换鱼的id {fishname}: {oldid} -> {newid}");
                ReplaceFishID(oldid, newid);
            }
        }
        Debug.Log($"替换鱼的id成功！！！{count}");
        SetAutoRefresh(true);
        AssetDatabase.Refresh();
    }

    static void ReplaceFishID(int oldid, int newid)
    {
        var cfg = SyncArtRes.ConfigModel.Instance.Get(oldid);
        if (cfg == null) return;
        string prefabName = $"Assets/GameData/{cfg.dir}/{cfg.model}/Prefabs/{cfg.model}.prefab";
        Debug.Log(prefabName);
        if (!File.Exists(prefabName))
        {
            Debug.LogError($"{prefabName} 不存在");
            return;
        }
        string dir = $"Assets/GameData/{cfg.dir}/{cfg.model}/";
        string[] files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
        int shortid = oldid % 1000;
        if (shortid < 100)
        {
            Debug.LogError($"旧的鱼id {oldid} {newid} 后三位小于100");
            mEnding = true;
            return;
        }
        string shortIDOld = shortid.ToString();
        shortid = newid % 1000;
        if (shortid < 100)
        {
            Debug.LogError($"新的鱼id {oldid} {newid} 后三位小于100");
            mEnding = true;
            return;
        }
        string shortIDNew = shortid.ToString();
        for (int i = 0; i < files.Length; i++)
        {
            string path = files[i];
            if (path.EndsWith(".meta"))
            {
                continue;
            }
            string filename = Path.GetFileName(path);
            if (filename.Contains(shortIDOld))
            {
                string newFilename = filename.Replace(shortIDOld, shortIDNew);
                string newPath = path.Replace(filename, newFilename);
                Debug.Log(path);
                Debug.Log(newPath);
                string error = AssetDatabase.RenameAsset(path, newFilename);
                Debug.Log(error);
            }
        }
        //mEnding = true;
    }

    #endregion

    [MenuItem("Tools/Art/替换鱼的目录")]
    public static void DoReplaceDir()
    {
        SetAutoRefresh(false);
        string filePath = "Assets/GameData/Excel~/微乐4.0新旧版鱼ID.xlsx";
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet result = excelReader.AsDataSet();
        var table = result.Tables[0];
        int columnNum = table.Columns.Count;
        int rowNum = table.Rows.Count;
        int count = 0;
        string text = "";
        for (int i = 1; i < rowNum; i++)
        {
            if (mEnding)
            {
                break;
            }
            string oldIDStr = table.Rows[i][0].ToString();
            string newIDStr = table.Rows[i][1].ToString();
            string fishname = table.Rows[i][2].ToString();
            if (string.IsNullOrEmpty(oldIDStr) || string.IsNullOrEmpty(newIDStr))
            {
                continue;
            }
            int oldid = 0;
            int newid = 0;
            int.TryParse(oldIDStr, out oldid);
            int.TryParse(newIDStr, out newid);
            if (oldid == 0 || newid == 0)
            {
                continue;
            }
            if (oldid == 100000306)
            {
                continue;
            }
            
            if (oldid != newid)
            {
                count++;
                var cfg = SyncArtRes.ConfigModel.Instance.Get(oldid);
                if (cfg == null)
                {
                    continue;
                }
                string dir = $"Assets/GameData/{cfg.dir}/{cfg.model}/";
                if (!Directory.Exists(dir))
                {
                    continue;
                }
                int shortid = oldid % 1000;
                if (shortid < 100)
                {
                    Debug.LogError($"旧的鱼id {oldid} {newid} 后三位小于100");
                    mEnding = true;
                    continue;
                }
                string shortIDOld = shortid.ToString();
                shortid = newid % 1000;
                if (shortid < 100)
                {
                    Debug.LogError($"新的鱼id {oldid} {newid} 后三位小于100");
                    mEnding = true;
                    continue;
                }
                string shortIDNew = shortid.ToString();
                string newDir = $"Assets/GameData/{cfg.dir}/{cfg.model.Replace(shortIDOld, shortIDNew)}";
                text += $"\n{dir}\n{newDir}\n";
                //DirectoryInfo
            }
            
        }
        File.WriteAllText("ReplaceFishDir.txt", text);
        Debug.Log($"替换鱼的目录成功！！！{count}");
        SetAutoRefresh(true);
        AssetDatabase.Refresh();
    }
}
