using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;



public class Tools_For_Art : MonoBehaviour
{
    [MenuItem("Tools/Art/关闭mipMap")]
    static void EnableTextureMipMap()
    {
        FixAssets("Res");
    }

    static void FixAssets(string strName)
    {
        var rootPath = Path.Combine(Application.dataPath, "GameData");
        LogUtils.I("FixAssets rootPath:" + rootPath);
        string[] pathList = Directory.GetDirectories(rootPath);
        for (int i = 0; i < pathList.Length; i++)
        {
            string path1 = pathList[i];
            if (path1.IndexOf(strName) < 0)
                continue;

            string[] pathList2 = Directory.GetDirectories(path1);
            for(int j = 0; j < pathList2.Length; j++)
            {
                string path2 = pathList2[j];

                LogUtils.I("FixAssets:" + path2);
                string[] files = Directory.GetFiles(path2, "*.png", SearchOption.AllDirectories);
                for (int k = 0; k < files.Length; k++)
                {
                    var file = files[k];
                    var f = file.Replace(Application.dataPath, "Assets");
                    TextureImporter importer = AssetImporter.GetAtPath(f) as TextureImporter;
                    if (importer == null)
                        continue;

                    if (importer.textureType == TextureImporterType.Sprite)
                    {
                        LogUtils.W("FixAssets sprite unwanted to set mipmap:" + f);
                        continue;
                    }

                    
                    // 不使用mipmap
                    if (importer.mipmapEnabled)
                    {
                        LogUtils.I("FixAssets enable mipmap:" + f);
                        importer.mipmapEnabled = false;
                        importer.filterMode = FilterMode.Bilinear;
                        importer.SaveAndReimport();
                    }
                }
            }
        }
    }

    //[MenuItem("Tools/Art/移除当前选中FBX中的Skinned Mesh Renderer中的默认材质")]
    static void Delete_Fbx_SkinnedMeshRenderer_Mat()
    {
        UnityEngine.Object[] arr = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);


        GameObject modle = (GameObject)arr[0];

        Debug.Log(modle.name);
        Material mat = null;
        var renderers = modle.GetComponentsInChildren<Renderer>();
        if (renderers == null)
        {
            return;

        }

        foreach (var renderer in renderers)
        {
            if (renderer == null)
            {
                continue;

            }
            renderer.sharedMaterial = mat;
        }



    }


    //[MenuItem("Tools/Art/移除GameData目录下全部FBX中的Skinned Mesh Renderer中的默认材质")]
    static void Delete_Fbx_SkinnedMeshRenderer_Mat_Batch()
    {


        string fullPath = "Assets/GameData/";

        if (Directory.Exists(fullPath))
        {
            DirectoryInfo direction = new DirectoryInfo(fullPath);
            FileInfo[] files = direction.GetFiles("*.fbx", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                //if (files[i].Name.EndsWith(".FBX"))
                {




                    Debug.Log("删除文件:" + files[i].Name + "中的Skinned Mesh Renderer中的默认材质");



                    string[] fileName_split = files[i].DirectoryName.Split(new[] { "\\Assets\\GameData\\" }, StringSplitOptions.None);

                    string Assets_path = "Assets\\GameData\\" + fileName_split[1] + "\\" + files[i].Name;

                    Assets_path = Assets_path.Replace("\\", "/");

                    GameObject GameObject_now = (GameObject)AssetDatabase.LoadAssetAtPath(Assets_path, typeof(GameObject));





                    GameObject modle = GameObject_now;

                    Material mat = null;
                    var renderers = modle.GetComponentsInChildren<Renderer>();
                    if (renderers == null)
                    {
                        return;

                    }

                    foreach (var renderer in renderers)
                    {
                        if (renderer == null)
                        {
                            continue;

                        }
                        renderer.sharedMaterial = mat;
                    }

                    //var importer = AssetImporter.GetAtPath(Assets_path);
                    //importer.SaveAndReimport();
                    //AssetDatabase.Refresh();

                }



            }


        }

    }

    [MenuItem("Tools/Art/清除材质球上的无效纹理")]
    public static void Clean()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo("Assets/GameData");
        FileInfo[] files = directoryInfo.GetFiles("*.mat", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            string fullName = files[i].FullName.Replace("\\", "/");
            fullName = fullName.Replace(Application.dataPath, "Assets");
            Debug.Log(fullName);
            CleanOneMaterial(AssetDatabase.LoadAssetAtPath<Material>(fullName));
        }

        Material[] materials = Selection.GetFiltered<Material>(SelectionMode.Assets | SelectionMode.DeepAssets);
        foreach (var material in materials)
        {
            CleanOneMaterial(material);
        }
    }

    private static bool CleanOneMaterial(Material _material)
    {
        // 收集材质使用到的所有纹理贴图
        HashSet<string> textureGUIDs = CollectTextureGUIDs(_material);

        string materialPathName = Path.GetFullPath(AssetDatabase.GetAssetPath(_material));

        StringBuilder strBuilder = new StringBuilder();
        using (StreamReader reader = new StreamReader(materialPathName))
        {
            Regex regex = new Regex(@"\s+guid:\s+(\w+),");
            string line = reader.ReadLine();
            while (null != line)
            {
                if (line.Contains("m_Texture:"))
                {
                    // 包含纹理贴图引用的行，使用正则表达式获取纹理贴图的guid
                    Match match = regex.Match(line);
                    if (match.Success)
                    {
                        string textureGUID = match.Groups[1].Value;
                        if (textureGUIDs.Contains(textureGUID))
                        {
                            strBuilder.AppendLine(line);
                        }
                        else
                        {
                            // 材质没有用到纹理贴图，guid赋值为0来清除引用关系
                            strBuilder.AppendLine(line.Substring(0, line.IndexOf("fileID:") + 7) + " 0}");
                        }
                    }
                    else
                    {
                        strBuilder.AppendLine(line);
                    }
                }
                else
                {
                    strBuilder.AppendLine(line);
                }

                line = reader.ReadLine();
            }
        }

        using (StreamWriter writer = new StreamWriter(materialPathName))
        {
            writer.Write(strBuilder.ToString());
        }

        return true;
    }

    private static HashSet<string> CollectTextureGUIDs(Material _material)
    {
        HashSet<string> textureGUIDs = new HashSet<string>();
        for (int i = 0; i < ShaderUtil.GetPropertyCount(_material.shader); ++i)
        {
            if (ShaderUtil.ShaderPropertyType.TexEnv == ShaderUtil.GetPropertyType(_material.shader, i))
            {
                Texture texture = _material.GetTexture(ShaderUtil.GetPropertyName(_material.shader, i));
                if (null == texture)
                {
                    continue;
                }

                string textureGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(texture));
                if (!textureGUIDs.Contains(textureGUID))
                {
                    textureGUIDs.Add(textureGUID);
                }
            }
        }

        return textureGUIDs;
    }

}
