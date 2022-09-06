using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Excel;
using System.Data;
using System.Text.RegularExpressions;

class FenBao
{
    [MenuItem("Tools/分包/按配表移动文件夹")]
    public static void ExportRes()
    {
        string filePath = "Assets/GameData/Excel~/鱼资源表.xlsx";
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet result = excelReader.AsDataSet();
        var table = result.Tables[0];
        int columnNum = table.Columns.Count;
        int rowNum = table.Rows.Count;

        var dic = new Dictionary<string, string>();
        for (int i = 2; i < rowNum; i++)
        {
            string str1 = table.Rows[i][0].ToString();
            if (string.IsNullOrEmpty(str1))
            {
                continue;
            }
            int id = 0;
            if (!int.TryParse(str1, out id))
            {
                Debug.LogError($"鱼资源表错误 {i} {str1}");
                return;
            }
            string name = table.Rows[i][2].ToString();
            string dir = table.Rows[i][3].ToString();
            dic[name] = dir;
            //Debug.Log($"{dir}  {name}");
        }

        string searchDir = Path.GetFullPath(Application.dataPath + "/GameData");
        foreach (string subDir in Directory.GetDirectories(searchDir, "*.*", SearchOption.TopDirectoryOnly))
        {
            if (subDir.Contains("RoomRes"))
            {
                foreach (string k in Directory.GetDirectories(subDir, "*.*", SearchOption.TopDirectoryOnly))
                {
                    string secondSubDir = k;
                    if (Regex.IsMatch(secondSubDir, "Fish[0-9]{3}$"))
                    {
                        secondSubDir = secondSubDir.Replace("\\", "/");
                        string[] array = secondSubDir.Split('/');
                        string name = array[array.Length - 1];
                        string dir = array[array.Length - 2];
                        //Debug.Log($"{name} {dir}");
                        if (dic.ContainsKey(name) && dic[name] != dir)
                        {
                            string newDir = secondSubDir.Replace(dir, dic[name]);
                            Debug.Log($"工程目录和配置不一致 {name} {dir}");
                            Debug.Log(secondSubDir);
                            Debug.Log(newDir);
                            CopyDirectory(secondSubDir, newDir);
                            Directory.Delete(secondSubDir, true);
                            Debug.Log($"同步成功{name}");
                        }
                    }
                }
            }
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
