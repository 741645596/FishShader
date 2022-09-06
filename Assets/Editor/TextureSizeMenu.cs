using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class TextureSizeMenu
{

    public static List<string> _listPaths = new List<string>();
    private static int _defaultPixelType = 0;
    private static bool _isToBig = true; //向着更大缩放

    [MenuItem("Assets/设置纹理最大尺寸/2048")]
    static void DoIt2048()
    {
        DoIt(2048);
    }

    [MenuItem("Assets/设置纹理最大尺寸/1024")]
    static void DoIt1024()
    {
        DoIt(1024);
    }

    [MenuItem("Assets/设置纹理最大尺寸/512")]
    static void DoIt512()
    {
        DoIt(512);
    }

    [MenuItem("Assets/设置纹理最大尺寸/256")]
    static void DoIt256()
    {
        DoIt(256);
    }

    [MenuItem("Assets/设置纹理最大尺寸/128")]
    static void DoIt128()
    {
        DoIt(128);
    }

    static void DoIt(int maxSize)
    {
        string[] guids = UnityEditor.Selection.assetGUIDs;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string[] sprite_guids = AssetDatabase.FindAssets("t:Texture ", new string[] { path });
            if (sprite_guids.Length != 0)
            {
                for (int j = 0; j < sprite_guids.Length; j++)
                {
                    string sp_path = AssetDatabase.GUIDToAssetPath(sprite_guids[j]);
                    SetTextureMaxSize(sp_path, maxSize);
                }
            }
            else
            {
                SetTextureMaxSize(path, maxSize);
            }
        }
        AssetDatabase.Refresh();
    }

    static void SetTextureMaxSize(string path, int maxSize)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        if (assetImporter == null)
        {
            return;
        }
        TextureImporter textureImporter = assetImporter as TextureImporter;
        if (textureImporter == null)
        {
            return;
        }
        textureImporter.maxTextureSize = maxSize;

        var iosTextureSettings = textureImporter.GetPlatformTextureSettings("iOS");
        if (iosTextureSettings != null)
        {
            iosTextureSettings.maxTextureSize = maxSize;
            try
            {
                textureImporter.SetPlatformTextureSettings(iosTextureSettings);
            }
            catch (Exception e)
            {
                Debug.Log(textureImporter.assetPath);
                Console.WriteLine(e);
                throw;
            }
        }

        var androidTextureSettings = textureImporter.GetPlatformTextureSettings("Android");
        if (androidTextureSettings != null)
        {
            androidTextureSettings.maxTextureSize = maxSize;
            try
            {
                textureImporter.SetPlatformTextureSettings(androidTextureSettings);
            }
            catch (Exception e)
            {
                Debug.Log(textureImporter.assetPath);
                Console.WriteLine(e);
                throw;
            }
        }

        AssetDatabase.WriteImportSettingsIfDirty(textureImporter.assetPath);

    }

}
