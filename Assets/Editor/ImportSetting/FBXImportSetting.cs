using System.IO;
using UnityEngine;
using System.Collections;
using UnityEditor;
using LitJson;


using System.Collections.Generic;

using UnityEngine.UI;

public class FBXImportSetting : AssetPostprocessor
{
    List<string> NotOptFBXs = new List<string>();
    List<string> NotNeedScaleCurve = new List<string>()
    {
        "Fish403",
        "Fish407",
        "Fish409",
        "Fish387",
        "Fish367",
        "Fish352",
        "Fish353",
        "Fish354",
        "Fish355",
        "Fish357",


        "Fish375",
        "Fish376",
        "Fish377",
        "Fish379",
        "Fish380",
        
        "Fish429",
        "Fish361",
        "Fish363",
        "Fish366",
        "Fish381",
        "Fish378",
        "Fish373",
        "Fish371",
        "Fish372",
        "Fish365",

        "Fish421",
        "Fish422",
        "Fish415",
        "Fish362",
        "Fish364",
        "Fish388",
        "Fish389"
    };

    void OnPostprocessModel(GameObject g)
    {
        HandleDeleteFbxMaterials(g);
        if (!IsNeedOpt(assetPath))
        {
            return;
        }

        Debug.Log("+++优化动画精度      " + assetPath);
        List<AnimationClip> animationClipList = new List<AnimationClip>(AnimationUtility.GetAnimationClips(g));
        if (animationClipList.Count == 0)
        {
            AnimationClip[] objectList = UnityEngine.Object.FindObjectsOfType(typeof(AnimationClip)) as AnimationClip[];
            animationClipList.AddRange(objectList);
        }

        foreach (AnimationClip theAnimation in animationClipList)
        {
            try
            {
                if (IsNotNeedScaleCurve(assetPath))
                {
                    Debug.Log("---去除Scale曲线      " + assetPath);
                    //去除scale曲线
                    foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(theAnimation))
                    {
                        string name = theCurveBinding.propertyName.ToLower();
                        if (name.Contains("scale"))
                        {
                            AnimationUtility.SetEditorCurve(theAnimation, theCurveBinding, null);
                        }
                    }
                }

                //浮点数精度压缩到f3
                AnimationClipCurveData[] curves = null;
                curves = AnimationUtility.GetAllCurves(theAnimation);
                Keyframe key;
                Keyframe[] keyFrames;
                for (int ii = 0; ii < curves.Length; ++ii)
                {
                    AnimationClipCurveData curveDate = curves[ii];
                    if (curveDate.curve == null || curveDate.curve.keys == null)
                    {
                        //Debug.LogWarning(string.Format("AnimationClipCurveData {0} don't have curve; Animation name {1} ", curveDate, animationPath));
                        continue;
                    }
                    keyFrames = curveDate.curve.keys;
                    for (int i = 0; i < keyFrames.Length; i++)
                    {
                        key = keyFrames[i];
                        key.value = float.Parse(key.value.ToString("f3"));
                        key.inTangent = float.Parse(key.inTangent.ToString("f3"));
                        key.outTangent = float.Parse(key.outTangent.ToString("f3"));
                        keyFrames[i] = key;
                    }
                    curveDate.curve.keys = keyFrames;
                    theAnimation.SetCurve(curveDate.path, curveDate.type, curveDate.propertyName, curveDate.curve);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("CompressAnimationClip Failed !!! animationPath : {0} error: {1}", assetPath, e));
            }
        }
    }

    void HandleDeleteFbxMaterials(GameObject model)
    {
        ModelImporter modelImp = (ModelImporter)assetImporter;
        //modelImp.materialImportMode = ModelImporterMaterialImportMode.None;
        string path = assetPath.ToLower();
        bool exist = false;
        Renderer[] renderComs = model.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderComs.Length; i++)
        {
            renderComs[i].sharedMaterials = new Material[renderComs[i].sharedMaterials.Length];
            exist = true;
        }
        if (exist)
        {
            Debug.Log($"删除默认材质 {path}");
        }
    }

    bool IsNeedOpt(string path)
    {
        for (int i = 0; i < NotOptFBXs.Count; i++)
        {
            if (path.Contains(NotOptFBXs[i]))
            {
                return false;
            }
        }
        return true;
    }

    bool IsNotNeedScaleCurve(string path)
    {
        return false;
        for (int i = 0; i < NotNeedScaleCurve.Count; i++)
        {
            if (path.Contains(NotNeedScaleCurve[i]))
            {
                return true;
            }
        }
        return false;
    }
}