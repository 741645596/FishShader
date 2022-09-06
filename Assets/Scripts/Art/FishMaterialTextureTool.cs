using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class FishMaterialTextureTool
{

    public static void ChangeTexture(SkinnedMeshRenderer renderer, string key, int size)
    {
#if UNITY_EDITOR
        var mat = renderer.materials[0];

        var texture = mat.GetTexture(key);
        var path = AssetDatabase.GetAssetPath(texture);
        Debug.Log(path);

        var token = path.Split('@');
        var dir = Path.GetDirectoryName(token[0]);
        var name = Path.GetFileName(token[0]).Split('.')[0];
        Debug.Log(dir);
        Debug.Log(name);

        var newPath = Path.Combine(dir, $"{name}@{size}.png");
        var newTexture = AssetDatabase.LoadAssetAtPath<Texture>(newPath);
        if(newTexture == null)
        {
            LogUtils.W($"找不到{newPath}");
            return;
        }

        mat.SetTexture(key, newTexture);
#endif
    }
}