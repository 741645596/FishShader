using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Excel;
using System.Data;

class SyncArtRes
{
    // 模型配置
    public class ConfigModel
    {
        public class CfgModel
        {
            public int id;
            public string name;
            public string model;
            public string dir;
        }
        static ConfigModel sInstance;
        public static ConfigModel Instance { get { if (sInstance == null) { sInstance = new ConfigModel(); } return sInstance; } }

        Dictionary<int, CfgModel> m_DataDic;
        List<int> m_AllKeys;
        public ConfigModel()
        {
            m_DataDic = new Dictionary<int, CfgModel>();
            m_AllKeys = new List<int>();
            var textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GameData/AppRes/DataBin/fish_model.json");
            var text = textAsset.text;
            List<CfgModel> list = LitJson.JsonMapper.ToObject<List<CfgModel>>(text);
            for (int i = 0; i < list.Count; i++)
            {
                var cfg = list[i];
                m_AllKeys.Add(cfg.id);
                m_DataDic[cfg.id] = cfg;
            }
        }

        public CfgModel Get(int id)
        {
            CfgModel ret = null;
            m_DataDic.TryGetValue(id, out ret);
            return ret;
        }

        public List<int> GetAllKeys()
        {
            return m_AllKeys;
        }

        public static void Dispose()
        {
            if (sInstance != null)
            {
                sInstance.m_DataDic = null;
                sInstance.m_AllKeys = null;
                sInstance = null;
            }
        }

    }

    [MenuItem("Tools/Art/导入鱼文字资源")]
    public static void Sync()
    {
        ImportPng("/../Art/美术文档/UI资源/F房间/彩盘/彩盘文字", "cp");
        ImportPng("/../Art/美术文档/UI资源/F房间/功能鱼来了/功能鱼文字", "gny");
        ImportPng("/../Art/美术文档/UI资源/F房间/BOSS来袭/BOSS来袭文字", "bscw");
        ConfigModel.Dispose();
    }

    static void ImportPng(string srcDir, string pre)
    {
        string root = Path.GetFullPath(Application.dataPath + srcDir);
        Debug.Log(root);
        if (Directory.Exists(root) == false)
        {
            Debug.LogError("请同步Art子模块");
            return;
        }
        foreach (string newPath in Directory.GetFiles(root, "*.png", SearchOption.TopDirectoryOnly))
        {
            string result = System.Text.RegularExpressions.Regex.Replace(newPath, @"[^0-9]+", "");
            if (string.IsNullOrEmpty(result)) continue;
            int fishid;
            if (!int.TryParse(result, out fishid)) continue;
            string dir = GetFishDir(fishid);
            if (string.IsNullOrEmpty(dir)) continue;
            if (Directory.Exists(dir))
            {
                CopyImageTo(newPath, dir, fishid, pre);
            }
            else
            {
                Debug.LogError($"无效的路径 {dir}");
            }
        }
    }

    static void CopyImageTo(string from, string dir, int fishid, string pre)
    {
        string uidir = dir + "/UI";
        if (!Directory.Exists(uidir))
        {
            Directory.CreateDirectory(uidir);
        }
        string dstPath = uidir + $"/{pre}_{fishid - 100000000}.png";
        File.Copy(from, dstPath, true);
        Debug.Log(dstPath);
        AssetDatabase.Refresh();
        TextureImporter importer = AssetImporter.GetAtPath(dstPath) as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.SaveAndReimport();
        Debug.Log("Success");
    }

    static string GetFishDir(int fishid)
    {
        var cfg = ConfigModel.Instance.Get(fishid);
        if (cfg == null) return "";
        return $"Assets/GameData/{cfg.dir}/{cfg.model}/";
    }
}
