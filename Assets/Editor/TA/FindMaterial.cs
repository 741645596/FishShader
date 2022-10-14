using UnityEditor;
using UnityEngine;

public class FindMaterial 
{
    [MenuItem("Tools/Shader查找")]
    static void FindShader()
    {
        string[] guids = AssetDatabase.FindAssets("t:Shader", new[] { "Assets/GameData/Shaders/wb_shader/" });
        foreach (string guid in guids)
        {
            string strPath = AssetDatabase.GUIDToAssetPath(guid);
            Shader t = AssetDatabase.LoadAssetAtPath<Shader>(strPath);
            Debug.Log(t.name, t);
        }
    }




    [MenuItem("Tools/材质查找")]
    static void Find()
    {
        Vector4 defalut = new Vector4(-1, -1, 0, 0);
        string[] guids = AssetDatabase.FindAssets("t:Material", null);
        foreach (string guid in guids)
        {
            string strPath = AssetDatabase.GUIDToAssetPath(guid);
            Material t = AssetDatabase.LoadAssetAtPath<Material>(strPath);
            if (t.shader.name == "Fish/ShaderGraph_Role" && t.GetFloat("_HitColorChannel") != 0)
            {
                //Vector4 v = t.GetVector("_MainSpeed");
                //if (v.w != 0)
                {
                    Debug.Log(strPath, t);
                }
            }
        }
    }
}




