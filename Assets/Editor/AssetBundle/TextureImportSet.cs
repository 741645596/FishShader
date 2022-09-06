using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEditor;
using UnityEngine;
using YamlDotNet.RepresentationModel;

public class TextureImportSet
{
    private List<string> packingFolderFilterList; // 过滤不需要成图集的文件夹

    private List<string> astc6x6TextureFormatFileList; // 指定文件使用astc6x6

    private List<string> astc5x5TextureFormatFolderList;
    private List<string> astc6x6TextureFormatFolderList;
    private List<string> astc8x8TextureFormatFolderList;
    private List<string> unityTextureFormatFolderList; // 使用unity配置的纹理格式

    public void InitConfigList()
    {
        packingFolderFilterList = new List<string>();
        astc5x5TextureFormatFolderList = new List<string>();
        astc6x6TextureFormatFolderList = new List<string>();
        astc8x8TextureFormatFolderList = new List<string>();
        astc6x6TextureFormatFileList = new List<string>();
        unityTextureFormatFolderList = new List<string>();

        string filterConfigPath = "Assets/GameData/BuildConfig~/TextureFormatConfig.json";
        if (!File.Exists(filterConfigPath))
        {
            Debug.LogWarning("错误提示：AssetCheckConfig.json文件在Asserts目录下不存在");
            return;
        }

        using (FileStream stream = File.OpenRead(filterConfigPath))
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string context = reader.ReadToEnd();
                JsonData config = JsonMapper.ToObject(context);
                if (!config.IsNull("NotPackingSpriteFolder"))
                {
                    foreach (var key in config["NotPackingSpriteFolder"])
                    {
                        packingFolderFilterList.Add(key.ToString());
                    }
                }
                else
                {
                    Debug.LogWarning("错误提示：AssetCheckConfig.json不存在参数NotPackingSpriteFolder，该参数用来过滤哪些文件夹不设置为AB包");
                }

                if (!config.IsNull("astc5x5TextureFormatFolder"))
                {
                    foreach (var key in config["astc5x5TextureFormatFolder"])
                    {
                        astc5x5TextureFormatFolderList.Add(key.ToString());
                    }
                }
                else
                {
                    Debug.LogWarning("错误提示：AssetCheckConfig.json不存在参数astc5x5TextureFormatFolder");
                }

                if (!config.IsNull("astc6x6TextureFormatFolder"))
                {
                    foreach (var key in config["astc6x6TextureFormatFolder"])
                    {
                        astc6x6TextureFormatFolderList.Add(key.ToString());
                    }
                }
                else
                {
                    Debug.LogWarning("错误提示：AssetCheckConfig.json不存在参数astc6x6TextureFormatFolder");
                }

                if (!config.IsNull("astc8x8TextureFormatFolder"))
                {
                    foreach (var key in config["astc8x8TextureFormatFolder"])
                    {
                        astc8x8TextureFormatFolderList.Add(key.ToString());
                    }
                }
                else
                {
                    Debug.LogWarning("错误提示：AssetCheckConfig.json不存在参数astc8x8TextureFormatFolder");
                }

                if (!config.IsNull("astc6x6TextureFormatFile"))
                {
                    foreach (var key in config["astc6x6TextureFormatFile"])
                    {
                        astc6x6TextureFormatFileList.Add(key.ToString());
                    }
                }
                else
                {
                    Debug.LogWarning("错误提示：AssetCheckConfig.json不存在参数astc6x6TextureFormatFile");
                }

                if (!config.IsNull("unityTextureFormatFolder"))
                {
                    foreach (var key in config["unityTextureFormatFolder"])
                    {
                        unityTextureFormatFolderList.Add(key.ToString());
                    }
                }
                else
                {
                    Debug.LogWarning("错误提示：AssetCheckConfig.json不存在参数unityTextureFormatFolder");
                }
            }
        }
    } //InitConfigList

    // 是否打图集
    private bool IsPackingSprite(string dirPath)
    {
        if (IsPackingFilterFolder(dirPath))
        {
            return false;
        }

        return IsOnlyPngAndJPG(dirPath);
    }

    // 文件夹内是否只有png、jpg、meta格式的文件
    private bool IsOnlyPngAndJPG(string dirPath)
    {
        string[] files = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (file.Contains(".png") ||
                file.Contains(".PNG") ||
                file.Contains(".jpg") ||
                file.Contains(".JPG") ||
                file.Contains(".meta"))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    public bool IsPackingFilterFolder(string dirPath)
    {
        for (int i = 0; i < packingFolderFilterList.Count; i++)
        {
            if (dirPath.Contains(packingFolderFilterList[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsInUnityTextureFormat(string filePath)
    {
        for (int i = 0; i < unityTextureFormatFolderList.Count; i++)
        {
            if (filePath.StartsWith(unityTextureFormatFolderList[i], System.StringComparison.CurrentCulture))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAstc6x6TextureFormatFile(string filePath)
    {
        for (int i = 0; i < astc6x6TextureFormatFileList.Count; i++)
        {
            if (astc6x6TextureFormatFileList[i].Contains(filePath))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAstc5x5TextureFormatFolder(string filePath)
    {
        for (int i = 0; i < astc5x5TextureFormatFolderList.Count; i++)
        {
            if (filePath.StartsWith(astc5x5TextureFormatFolderList[i],
                System.StringComparison.CurrentCulture))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAstc6x6TextureFormatFolder(string filePath)
    {
        for (int i = 0; i < astc6x6TextureFormatFolderList.Count; i++)
        {
            if (filePath.StartsWith(astc6x6TextureFormatFolderList[i],
                System.StringComparison.CurrentCulture))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAstc8x8TextureFormatFolder(string filePath)
    {
        if (!filePath.Contains("Assets/GameData/CommonRes") && filePath.Contains("FXTextures"))
        {
            return true;
        }

        for (int i = 0; i < astc8x8TextureFormatFolderList.Count; i++)
        {
            if (filePath.StartsWith(astc8x8TextureFormatFolderList[i],
                System.StringComparison.CurrentCulture))
            {
                return true;
            }
        }

        return false;
    }

    public void RemoveTextureOverride(TextureImporter textureImporter)
    {
        TextureImporterPlatformSettings androidSetting = new TextureImporterPlatformSettings();
        androidSetting.maxTextureSize = textureImporter.maxTextureSize;
        androidSetting.overridden = false;
        androidSetting.name = "Android";
        textureImporter.SetPlatformTextureSettings(androidSetting);

        TextureImporterPlatformSettings iSOSetting = new TextureImporterPlatformSettings();
        iSOSetting.maxTextureSize = textureImporter.maxTextureSize;
        iSOSetting.overridden = false;
        iSOSetting.name = "IOS";
        textureImporter.SetPlatformTextureSettings(iSOSetting);

        AssetDatabase.WriteImportSettingsIfDirty(textureImporter.assetPath);
    }

    public void SetTextureFormat(TextureImporter textureImporter)
    {
        // 默认的不处理
        var filePath = textureImporter.assetPath;
        filePath = filePath.Replace("\\", "/");
        
        if (IsInUnityTextureFormat(filePath))
        {
            return;
        }
        // fnt 都不压缩
        string filename = filePath.ToLower();
        if (filename.Contains("fnt"))
        {
            SetAstcFormat(textureImporter, TextureImporterFormat.ASTC_4x4);
            return;
        }

        // 鱼的模型贴图暂时不压缩
        //if (filePath.Contains("/RoomRes1/") && filePath.Contains("FishTextures"))
        //{
        //    SetAstcFormat(textureImporter, TextureImporterFormat.ASTC_4x4);
        //    return;
        //}
        // 法线贴图不压缩
        if (textureImporter.textureType == TextureImporterType.NormalMap)
        {
            SetAstcFormat(textureImporter, TextureImporterFormat.ASTC_4x4);
            return;
        }

        // 指定文件
        if (IsAstc6x6TextureFormatFile(filePath))
        {
            SetAstcFormat(textureImporter, TextureImporterFormat.ASTC_5x5);
            return;
        }

        // 指定目录
        if (IsAstc8x8TextureFormatFolder(filePath))
        {
            Debug.Log(filePath);
            SetAstcFormat(textureImporter, TextureImporterFormat.ASTC_8x8);
            return;
        }

        if (IsAstc6x6TextureFormatFolder(filePath))
        {
            SetAstcFormat(textureImporter, TextureImporterFormat.ASTC_6x6);
            return;
        }

        // 指定文件夹
        if (IsAstc5x5TextureFormatFolder(filePath))
        {
            SetAstcFormat(textureImporter, TextureImporterFormat.ASTC_5x5);
            return;
        }

        SetDefaultFormat(textureImporter);
    }

    // 默认6x6压缩
    private void SetDefaultFormat(TextureImporter textureImporter)
    {
        //SetAndroidTextureSetting(textureImporter, TextureImporterFormat.ETC2_RGBA8Crunched);
        SetAndroidTextureSetting(textureImporter, TextureImporterFormat.ASTC_6x6);
        SetIosTextureSetting(textureImporter, TextureImporterFormat.ASTC_6x6);

        AssetDatabase.WriteImportSettingsIfDirty(textureImporter.assetPath);
    }

    private void SetAstcFormat(TextureImporter textureImporter, TextureImporterFormat format)
    {
        SetAndroidTextureSetting(textureImporter, format);
        SetIosTextureSetting(textureImporter, format);

        AssetDatabase.WriteImportSettingsIfDirty(textureImporter.assetPath);
    }

    private void SetAndroidTextureSetting(TextureImporter textureImporter, TextureImporterFormat format)
    {
        TextureImporterPlatformSettings androidSetting = new TextureImporterPlatformSettings();
        androidSetting.overridden = true;
        androidSetting.name = "Android";
        androidSetting.maxTextureSize = textureImporter.maxTextureSize;
        androidSetting.format = format;

        try
        {
            textureImporter.SetPlatformTextureSettings(androidSetting);
        }
        catch (Exception e)
        {
            Debug.Log(textureImporter.assetPath);
            Console.WriteLine(e);
            throw;
        }
    }

    private void SetIosTextureSetting(TextureImporter textureImporter, TextureImporterFormat format)
    {
        TextureImporterPlatformSettings iOSSettings = new TextureImporterPlatformSettings();
        iOSSettings.overridden = true;
        iOSSettings.name = "iOS";
        iOSSettings.textureCompression = TextureImporterCompression.Uncompressed;
        iOSSettings.maxTextureSize = textureImporter.maxTextureSize;
        iOSSettings.format = format;
        try
        {
            textureImporter.SetPlatformTextureSettings(iOSSettings);
        }
        catch (Exception e)
        {
            Debug.Log(textureImporter.assetPath);
            Console.WriteLine(e);
            throw;
        }
    }

    public static List<String> TexturesInAtlas(String spriteAtlasPath)
    {
        List<String> texs = new List<string>();
        StreamReader yamlReader = File.OpenText(spriteAtlasPath);

        var yaml = new YamlStream(); // Load the stream
        try
        {
            yaml.Load(yamlReader);
        }
        catch (Exception e)
        {
            Debug.LogError("Read TexturesInAtlas failed:" + spriteAtlasPath);
            Console.WriteLine(e);
            return texs;
        }
        finally
        {
            yamlReader.Close();
        }

        var yamlNode = (YamlSequenceNode)yaml.Documents[0].RootNode["SpriteAtlas"]["m_PackedSprites"];

        foreach (var node in yamlNode)
        {
            var yamlNode1 = node["guid"];
            texs.Add(yamlNode1.ToString());
        }

        return texs;
    }


    public void SetDefaultTextureCompressOpt(List<string> guidsInAtlas, TextureImporter textureImporter)
    {
        if (textureImporter == null)
            return;

        string relatPath = textureImporter.assetPath;
        if (!relatPath.Contains("/GameData/"))
            return;
        var guidFromAssetPath = AssetDatabase.GUIDFromAssetPath(relatPath);
        var strGUID = guidFromAssetPath.ToString();
        if (guidsInAtlas.Contains(strGUID))
        {
            RemoveTextureOverride(textureImporter);
        }
        else
        {
            TextureImporterPlatformSettings androidPlatformSetting = textureImporter.GetPlatformTextureSettings("Android");
            TextureImporterPlatformSettings iosPlatformSetting = textureImporter.GetPlatformTextureSettings("iOS");
            if (androidPlatformSetting == null || iosPlatformSetting == null)
            {
                SetAstcFormat(textureImporter, TextureImporterFormat.ASTC_6x6);
            }
        }
    }

    public static void SetTextureCompressOpt(List<string> guidsInAtlas, TextureImporter textureImporter,
        TextureImportSet textureImportSet)
    {
        if (textureImporter == null)
            return;

        string relatPath = textureImporter.assetPath;
        if (!relatPath.Contains("/GameData/"))
            return;

        var guidFromAssetPath = AssetDatabase.GUIDFromAssetPath(relatPath);
        var strGUID = guidFromAssetPath.ToString();

        if (guidsInAtlas.Contains(strGUID))
            textureImportSet.RemoveTextureOverride(textureImporter);
        else
        {
            textureImportSet.SetTextureFormat(textureImporter);
        }
        // if (textureImportSet.IsPackingFilterFolder(relatPath)) {
        //     if (guidsInAtlas.Contains(strGUID))
        //         textureImportSet.RemoveTextureOverride(textureImporter);
        //     else {
        //         textureImportSet.SetTextureFormat(textureImporter);
        //     }
        // }
        // else {
        //     textureImportSet.SetTextureFormat(textureImporter);
        // }
    }
}
