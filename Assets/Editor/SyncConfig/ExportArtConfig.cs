using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Excel;
using System.Data;
using System.Text.RegularExpressions;

class ExportArtConfig
{
    [System.Serializable]
    class TmpClass
    {
        public int id;
        public int duration;
        public string path;
        public string desc;
    }

    [System.Serializable]
    public class TmpCfgModel
    {
        public int id;
        public string name;
        public string model;
        public string dir;
    }

    [System.Serializable]
    public class TmpCfgLanguage
    {
        public int id;
        public string chs;
        public string en;
    }

    static Dictionary<string, TmpCfgModel> s_FishDic;

    [MenuItem("Tools/同步配置/导出美术配表")]
    public static void Sync()
    {
        s_FishDic = new Dictionary<string, TmpCfgModel>();
        LitJson.JsonMapper.SetEnablePrettyPrint(true);
        ExportFish();
        ExportEffect();
        ExportAudio();
        LitJson.JsonMapper.SetEnablePrettyPrint(false);
        s_FishDic = null;
    }

    static void ExportEffect()
    {
        string filePath = "Assets/GameData/Excel~/特效资源表.xlsx";
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet result = excelReader.AsDataSet();
        var table = result.Tables[0];
        int columnNum = table.Columns.Count;
        int rowNum = table.Rows.Count;
        List<TmpClass> list = new List<TmpClass>();
        for (int i = 3; i < rowNum; i++)
        {
            var unit = new TmpClass();
            string str1 = table.Rows[i][1].ToString();
            if (string.IsNullOrEmpty(str1))
            {
                continue;
            }
            string str2 = table.Rows[i][2].ToString();
            string str3 = table.Rows[i][3].ToString();
            if (!int.TryParse(str1, out unit.id))
            {
                Debug.LogError($"导出特效资源表错误 {i} {str1}");
                break;
            }
            if (Regex.IsMatch(str2, "Fish[0-9]{3}"))
            {
                Match match = Regex.Match(str2, "Fish[0-9]{3}");
                var fishname = match.Value;
                if (s_FishDic.ContainsKey(fishname))
                {
                    str2 = s_FishDic[fishname].dir + "/" + str2;
                    Debug.Log("特效路径 " + s_FishDic[fishname].dir + " " + str2);
                }
            }

            unit.path = str2;
            float duration = 0;
            if (!float.TryParse(str3, out duration))
            {
                Debug.LogWarning($"特效时间配置错误 {i} {str3}");
            }
            if (duration > 0)
            {
                unit.duration = Mathf.FloorToInt(duration * 1000);
            }
            else
            {
                unit.duration = -1;
            }
            string str4 = table.Rows[i][4].ToString();
            unit.desc = str4;
            list.Add(unit);
        }
        string jsonStr = LitJson.JsonMapper.ToJson(list);
        string savePath = "Assets/GameData/AppRes/DataBin/effect.json";
        File.WriteAllText(savePath, jsonStr);
        Debug.Log("导出特效资源表成功！！！");
    }

    static void ExportAudio()
    {
        string filePath = "Assets/GameData/Excel~/音效资源表.xlsx";
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet result = excelReader.AsDataSet();
        var table = result.Tables[0];
        int columnNum = table.Columns.Count;
        int rowNum = table.Rows.Count;
        List<TmpClass> list = new List<TmpClass>();
        for (int i = 3; i < rowNum; i++)
        {
            var unit = new TmpClass();
            string str1 = table.Rows[i][0].ToString();
            if (string.IsNullOrEmpty(str1))
            {
                continue;
            }
            string str2 = table.Rows[i][1].ToString();
            if (!int.TryParse(str1, out unit.id))
            {
                Debug.LogError($"导出音效资源表错误 {i} {str1}");
                return;
            }
            if (Regex.IsMatch(str2, "Fish[0-9]{3}"))
            {
                Match match = Regex.Match(str2, "Fish[0-9]{3}");
                var fishname = match.Value;
                if (s_FishDic.ContainsKey(fishname))
                {
                    str2 = s_FishDic[fishname].dir + "/" + str2;
                    Debug.Log("音效路径 " + s_FishDic[fishname].dir + " " + str2);
                }
            }
            unit.path = str2;
            list.Add(unit);
        }
        string jsonStr = LitJson.JsonMapper.ToJson(list);
        string savePath = "Assets/GameData/AppRes/DataBin/audio.json";
        File.WriteAllText(savePath, jsonStr);
        Debug.Log("导出音效资源表成功！！！");
    }

    static void ExportFish()
    {
        string filePath = "Assets/GameData/Excel~/鱼资源表.xlsx";
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet result = excelReader.AsDataSet();
        var table = result.Tables[0];
        int columnNum = table.Columns.Count;
        int rowNum = table.Rows.Count;
        List<TmpCfgModel> list = new List<TmpCfgModel>();
        for (int i = 2; i < rowNum; i++)
        {
            var unit = new TmpCfgModel();
            string str1 = table.Rows[i][0].ToString();
            if (string.IsNullOrEmpty(str1))
            {
                continue;
            }
            if (!int.TryParse(str1, out unit.id))
            {
                Debug.LogError($"导出鱼资源表错误 {i} {str1}");
                return;
            }
            unit.name = table.Rows[i][1].ToString();
            unit.dir = table.Rows[i][2].ToString();
            int shortid = unit.id % 1000;

            unit.model = $"Fish{shortid.ToString("D3")}";
            list.Add(unit);
            s_FishDic[unit.model] = unit;
        }
        string jsonStr = LitJson.JsonMapper.ToJson(list);
        string savePath = "Assets/GameData/AppRes/DataBin/fish_model.json";
        File.WriteAllText(savePath, jsonStr);
        Debug.Log("导出鱼资源表成功！！！");
    }

    [MenuItem("Tools/导出语言表")]
    static void ExportLanguage()
    {
        string filePath = "Assets/GameData/Excel~/Language.xlsx";
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet result = excelReader.AsDataSet();
        var table = result.Tables[0];
        int columnNum = table.Columns.Count;
        int rowNum = table.Rows.Count;
        List<TmpCfgLanguage> list = new List<TmpCfgLanguage>();
        for (int i = 2; i < rowNum; i++)
        {
            var unit = new TmpCfgLanguage();
            string str1 = table.Rows[i][1].ToString();
            if (string.IsNullOrEmpty(str1))
            {
                continue;
            }
            if (!int.TryParse(str1, out unit.id))
            {
                Debug.LogError($"导出鱼资源表错误 {i} {str1}");
                return;
            }
            string str2 = table.Rows[i][2].ToString();
            unit.chs = str2;
            str2 = table.Rows[i][3].ToString();
            unit.en = str2;
            list.Add(unit);
        }
        string jsonStr = LitJson.JsonMapper.ToJson(list);
        string savePath = "Assets/GameData/AppRes/DataBin/language.json";
        File.WriteAllText(savePath, jsonStr);
        Debug.Log("导出语言表成功！！！");
    }
}
