using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class RightClickMenu
{

    public static List<string> _listPaths = new List<string>();
    private static int _defaultPixelType = 0;
    private static bool _isToBig = true; //向着更大缩放

    [MenuItem("Assets/UI尺寸改成4的倍数")]
    static void Div4Tex()
    {
        InitParams();
        GetFiles();
        ExFile();
    }

    public static void InitParams()
    {
        _listPaths.Clear();
        _isToBig = true;
        _defaultPixelType = 0;
    }

    public static void GetFiles()
    {
        string[] guids = UnityEditor.Selection.assetGUIDs;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string[] sprite_guids = AssetDatabase.FindAssets("t:sprite ", new string[] { path });
            if (sprite_guids.Length != 0)
            {
                for (int j = 0; j < sprite_guids.Length; j++)
                {
                    string sp_path = AssetDatabase.GUIDToAssetPath(sprite_guids[j]);
                    var sp = AssetDatabase.LoadAssetAtPath<Sprite>(sp_path);
                    bool isNeedDiv = sp.texture.width % 4 == 0 && sp.texture.height % 4 == 0;
                    if (isNeedDiv == false)
                    {
                        _listPaths.Add(sp_path);
                    }
                }
            }
            else
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sp)
                {
                    bool isNeedDiv = sp.texture.width % 4 == 0 && sp.texture.height % 4 == 0;
                    if (isNeedDiv == false)
                    {
                        _listPaths.Add(path);
                    }
                }
            }
        }
    }

    public static void ExTex(string path)
    {
        if (AssetDatabase.LoadMainAssetAtPath(path) is Texture2D tex)
        {
            // width整除4
            int pX = tex.width % 4;
            // height整除4
            int pY = tex.height % 4;

            if (pX == 0 && pY == 0)
            {
                return;
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            //更改图片属性，可读，否则无法获取Pixel
            importer.isReadable = true;
            importer.SaveAndReimport();

            Vector2Int v2 = GetFourSize(tex.width, tex.height);
            Texture2D texCopy = new Texture2D(v2.x, v2.y, TextureFormat.RGBA32, false, true);

            // 向右向下
            if (_defaultPixelType == 1)
            {
                var heightOffset = v2.y - tex.height;
                for (int i = 0; i < v2.x; i++)
                {
                    for (int j = v2.y - 1; j >= 0; j--)
                    {
                        var color = Color.white;
                        if (i >= tex.width || j <= heightOffset)
                        {
                            color.a = 0;
                        }
                        else
                        {
                            color = tex.GetPixel(i, j - heightOffset);
                        }
                        texCopy.SetPixel(i, j, color);
                    }
                }
            }
            // 向右向上
            else if (_defaultPixelType == 2)
            {
                for (int i = 0; i < v2.x; i++)
                {
                    for (int j = 0; j < v2.y; j++)
                    {
                        var color = Color.white;
                        if (i >= tex.width || j >= tex.height)
                        {
                            color.a = 0;
                        }
                        else
                        {
                            color = tex.GetPixel(i, j);
                        }
                        texCopy.SetPixel(i, j, color);
                    }
                }
            }
            // 中心点扩张
            else if (_defaultPixelType == 0)
            {

                // 上下
                int pXaddLeft = pY == 0 ? 0 : (4 - pY) / 2;
                int pXaddRight = pY == 0 ? 0 : 4 - pY - pXaddLeft;
                // 左右
                int pYaddLeft = pX == 0 ? 0 : (4 - pX) / 2;
                int pYaddRight = pX == 0 ? 0 : 4 - pX - pYaddLeft;

                for (int i = 0; i < v2.x; i++)
                {
                    for (int j = 0; j < v2.y; j++)
                    {
                        var color = Color.white;

                        if (j < pXaddLeft || j >= v2.y - pXaddRight)
                        {
                            color.a = 0;
                        }
                        else if (i < pYaddLeft || i >= v2.x - pYaddRight)
                        {
                            color.a = 0;
                        }
                        else
                        {
                            color = tex.GetPixel(i - pYaddLeft, j - pXaddLeft);
                        }

                        texCopy.SetPixel(i, j, color);
                    }
                }
            }

            texCopy.Apply();
            File.WriteAllBytes(path, texCopy.EncodeToPNG());
            //恢复不可读
            importer.isReadable = false;
            importer.SaveAndReimport();


        }
    }
    public static void ExFile()
    {
        try
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("处理过路径：");

            for (int i = 0; i < _listPaths.Count; i++)
            {
                EditorUtility.DisplayProgressBar("开始处理", $"{i + 1}/{_listPaths.Count}", (float)i / _listPaths.Count);
                string path = _listPaths[i];
                ExTex(path);

                builder.AppendLine($"{path}");
            }

            _listPaths.Clear();
            AssetDatabase.Refresh();

            Debug.Log(builder.ToString());
        }
        catch (Exception message)
        {
            Debug.LogError(message);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    /// <summary>
    /// 目标尺寸，宽高整数4处理
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Vector2Int GetFourSize(int width, int height)
    {
        if (_isToBig)
        {
            while (width % 4 != 0)
            {
                width++;
            }

            while (height % 4 != 0)
            {
                height++;
            }
        }
        else
        {
            while (width % 4 != 0)
            {
                width--;
            }

            while (height % 4 != 0)
            {
                height--;
            }
        }

        return new Vector2Int(Mathf.Max(4, width), Mathf.Max(4, height));
    }

}
