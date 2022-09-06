
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// 图集专有属性
/// </summary>
public class AtlasAssetInfo : AssetInfoBase
{
    // 图集是否存在
    public bool isSpriteAtlasExist;

    // 是否是ASTC格式
    public bool isAstcFormat;
    public TextureImporterFormat iosTextureFormat;
    public TextureImporterFormat androidTextureFormat;

    // 是否开启Override
    public bool isOpenOverride;

    // 原图是否都是Sprite（2D and UI）格式
    public bool isSprite2DFormat;

    // 原图是否都是ASTC 4x4格式或是未开启override
    public bool isAllASTC_4x4;

    // 图集路径
    public string spriteAtlasAssetPath;

    // 是否允许翻转
    public bool allowRotation;

    // 合图的资源文件集合
    public List<string> fileChilds;

    public override bool CanFix()
    {
        return IsError();
    }

    public override void Fix()
    {
        // 默认使用ASTC 5x5
        Fix(1);
    }

    public void Fix(int astcIndex)
    {
        if (isSprite2DFormat == false ||
            isAllASTC_4x4 == false)
        {
            _FixSprite2D();
        }

        if (isSpriteAtlasExist == false)
        {
            _FixCreateSpriteAtlas();
            return;
        }

        if (isAstcFormat == false ||
            isOpenOverride == false ||
            allowRotation == true)
        {
            _FixAstcFormat(astcIndex);
        }
    }

    /// <summary>
    /// 修复非ASTC格式
    /// </summary>
    /// <param name="info"></param>
    /// <param name="astcIndex"> 0:ASTC_4x4，1:ASTC_5x5，其他ASTC_6x6 </param>
    private void _FixAstcFormat(int astcIndex)
    {
        var format = _GetTextureFormat(astcIndex);
        SetAstcFormat(format);
    }

    /// <summary>
    /// 设置纹理格式
    /// </summary>
    /// <param name="info"></param>
    /// <param name="newFormat"></param>
    public void SetAstcFormat(TextureImporterFormat newFormat)
    {
        var res = AtlasChecker.SetASTC(spriteAtlasAssetPath, newFormat);
        if (res)
        {
            isAstcFormat = true;
            isOpenOverride = true;
            allowRotation = false;
            iosTextureFormat = newFormat;
            androidTextureFormat = newFormat;
        }
        else
        {
            Debug.LogError($"错误提示：修改{spriteAtlasAssetPath}ASTC格式失败，请检查");
        }
    }

    public override string GetErrorDes()
    {
        if (IsError() == false) return _GetASTCDes(this);

        var desArr = new List<string>();

        if (isSprite2DFormat == false)
        {
            desArr.Add("包含非Sprite(2D and UI)资源");
        }

        if (isAllASTC_4x4 == false)
        {
            desArr.Add("原图非ASTC 4x4");
        }

        if (allowRotation == true)
        {
            desArr.Add("不建议Allow Rotation");
        }

        if (isSpriteAtlasExist == false)
        {
            desArr.Add("SpriteAtlas不存在");
        }
        else
        {
            if (isAstcFormat == false)
            {
                desArr.Add("非ASTC格式");
            }
            else
            {
                var des = _GetASTCDes(this);
                desArr.Add(des);
            }

            if (isOpenOverride == false)
            {
                desArr.Add("Override未开启");
            }
        }

        return string.Join("；", desArr);
    }

    public override bool IsError()
    {
        if (isSpriteAtlasExist == false) return true;

        if (isSprite2DFormat == false) return true;

        if (isAllASTC_4x4 == false) return true;

        if (isAstcFormat == false) return true;

        if (isOpenOverride == false) return true;

        // 不允许翻转
        if (allowRotation == true) return true;

        return false;
    }

    private static string _GetASTCDes(AtlasAssetInfo info)
    {
        if (info.androidTextureFormat == info.iosTextureFormat)
        {
            return $"纹理格式:{BigPicChecker.GetAstcName(info.androidTextureFormat)}";
        }

        return $"android:{BigPicChecker.GetAstcName(info.androidTextureFormat)},ios:{BigPicChecker.GetAstcName(info.iosTextureFormat)}";
    }

    private static bool _SetSprite2DAndASTC4x4(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"错误提示：{assetPath}非TextureImporter格式，请检查资源");
            return false;
        }

        BigPicChecker.SetTextureFormat(importer, TextureImporterFormat.ASTC_4x4);

        importer.textureType = TextureImporterType.Sprite;

        importer.SaveAndReimport();
        return true;
    }

    private static TextureImporterFormat _GetTextureFormat(int astcIndex)
    {
        if (astcIndex == 0) return TextureImporterFormat.ASTC_4x4;
        if (astcIndex == 1) return TextureImporterFormat.ASTC_5x5;
        return TextureImporterFormat.ASTC_6x6;
    }

    private void _FixSprite2D()
    {
        foreach (var file in fileChilds)
        {
            _SetSprite2DAndASTC4x4(file);
        }
        isSprite2DFormat = AtlasChecker.IsAllSprite2DFormat(fileChilds);
        isAllASTC_4x4 = AtlasChecker.IsAllASTC4x4(fileChilds);
    }

    private void _FixCreateSpriteAtlas()
    {
        AtlasCreater.CreateAtlasAsset(assetPath);
        isSpriteAtlasExist = File.Exists(spriteAtlasAssetPath);
        isAstcFormat = true;
        isOpenOverride = true;
    }

}
