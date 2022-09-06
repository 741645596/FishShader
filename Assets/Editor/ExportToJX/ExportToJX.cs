using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

/*
class ExportToJX
{
    //[MenuItem("Tools/JX/导出吉祥4.0的鱼")]
    public static void ExportRes()
    {
        List<int> list = SyncArtRes.ConfigModel.Instance.GetAllKeys();
        string dstDir = $"{Application.dataPath}/../ExportFish/";
        CreateDir(dstDir);
        Dictionary<string, string> replaceDic = new Dictionary<string, string>();
        for (int i = 0; i < list.Count; i++)
        {
            string d1 = GetFishDir(list[i]);
            string d2 = $"fish_{list[i]}";
            Debug.Log($"拷贝目录 {d2}");
            replaceDic[d1] = d2;
            CopyDirectory("Assets/GameData/" + d1, dstDir + d2);
        }
        Dictionary<string, string> extends = new Dictionary<string, string>
        {
            { "CommonRes/BossRes", "commonres_boss"},
            { "CommonRes/MatCap", "commonres_mapcap"},
            { "CommonRes/FXMaterials", "commonres_materials"},
            { "CommonRes/FXModel", "commonres_model"},
            { "CommonRes/FXPublic", "commonres_public"},
            { "CommonRes/FXTextures", "commonres_textures"},
            { "AppRes/Audio/SanGuoSkillAudio", "san_guo_skill_audio"},

            { "AppRes/Audio/Caipan", "fish_n_audio/caipan"},
            { "AppRes/Audio/Fishdie", "fish_die_audio"},

            //{ "Shaders", "NewShaders"},
            { "RoomRes1/Map", "map1"},
            { "RoomRes2/Map", "map2"},
            { "RoomRes3/Map", "map3"},
            { "RoomRes4/Map", "map4"},
            { "RoomRes5/Map", "map5"},
            { "RoomRes6/Map", "map6"},

            { "SGRoomRes1/Map", "map_sg1"},
            { "SGRoomRes2/Map", "map_sg2"},
            { "SGRoomRes3/Map", "map_sg3"},
            { "SGRoomRes4/Map", "map_sg4"},
            { "SGRoomRes5/Map", "map_sg5"},
            { "SGRoomRes6/Map", "map_sg6"},

            { "RoomRes1/Fish306", "fish_100000306" },

            { "AppRes/DataBin", "databin"},
            { "RoomRes2/RoomEffect2", "roomeffect2"}
        };
        foreach(var key in extends)
        {
            string d1 = $"Assets/GameData/{key.Key}";
            string name = key.Value;
            if (name == "")
            {
                name = key.Key.ToLower();
            }
            replaceDic[key.Key] = name;
            string d2 = $"{dstDir}{name}";
            Debug.Log($"拷贝目录 {d2}");
            CopyDirectory(d1, d2);
        }
        // 同步美术配表
        SyncArtConfig(replaceDic, dstDir + "/databin/audio.json");
        SyncArtConfig(replaceDic, dstDir + "/databin/effect.json");
        SyncArtRes.ConfigModel.Dispose();
        ModifyMeta();
    }

    //[MenuItem("Tools/JX/测试")]
    static void ModifyMeta()
    {
        string dstDir = $"{Application.dataPath}/../ExportFish/";
        foreach (string newPath in Directory.GetDirectories(dstDir))
        {
            string name = newPath.Replace(dstDir, "");
            Debug.Log(name);
            foreach (string metaPath in Directory.GetFiles(newPath, "*.meta", SearchOption.AllDirectories))
            {
                string dir = metaPath.Replace(".meta", "");
                if (!Directory.Exists(dir))
                {
                    string text = File.ReadAllText(metaPath);
                    bool success = false;
                    var newText = Regex.Replace(text, @"assetBundleName: \S+", (Match mat) => {
                        //Debug.Log(mat.Value);
                        success = true;
                        return $"assetBundleName: {name}";
                    });
                    //Debug.Log(newText);
                    //break;
                    if (success)
                    {
                        File.WriteAllText(metaPath, newText);
                    }
                }
                
            }
            //break;
        }
    }

    //[MenuItem("Tools/JX/同步资源到吉祥")]
    static void SyncRes()
    {
        string dstDir = $"{Application.dataPath}/../ExportFish/";
        string jxDir = "D:/UnityFish/UnityRuntime/Assets/GameAssets/";
        foreach (string newPath in Directory.GetDirectories(dstDir))
        {
            string name = Path.GetFileName(newPath);
            string dir = jxDir + name;
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Debug.Log(name);
            CopyDirectory(newPath, dir);
        }
    }

    static string GetFishDir(int fishid)
    {
        var cfg = SyncArtRes.ConfigModel.Instance.Get(fishid);
        if (cfg == null) return "";
        return $"{cfg.dir}/{cfg.model}";
    }

    static void SyncArtConfig(Dictionary<string, string> replaceDic, string path)
    {
        string str = File.ReadAllText(path);
        foreach (var element in replaceDic)
        {
            string key = element.Key;
            string value = element.Value;
            str = str.Replace(key, value);
        }
        File.WriteAllText(path, str);
    }

    static void CreateDir(string dir)
    {
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }
    }

    static void CopyDirectory(string srcDir, string tgtDir)
    {
        DirectoryInfo source = new DirectoryInfo(srcDir);
        DirectoryInfo target = new DirectoryInfo(tgtDir);

        if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new Exception("父目录不能拷贝到子目录！");
        }

        if (!source.Exists)
        {
            return;
        }

        if (!target.Exists)
        {
            target.Create();
        }

        FileInfo[] files = source.GetFiles();

        for (int i = 0; i < files.Length; i++)
        {
            File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
        }

        DirectoryInfo[] dirs = source.GetDirectories();

        for (int j = 0; j < dirs.Length; j++)
        {
            CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
        }
    }
}
*/
