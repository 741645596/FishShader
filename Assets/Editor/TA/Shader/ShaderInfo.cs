using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Reflection;

public class ShaderInfo
{
    /// <summary>
    /// 是否支持SRP
    /// </summary>
    public bool isSupportSRP;
    /// <summary>
    /// 变体数量
    /// </summary>
    public int variantCount;
    /// <summary>
    /// 整个shader的属性数据分析
    /// </summary>
    public Dictionary<ShaderPropertyType, List<string>> m_DicProperty = new Dictionary<ShaderPropertyType, List<string>>();
    /// <summary>
    /// 清理数据
    /// </summary>
    public void Clear()
    {
        if (m_DicProperty != null)
        {
            foreach (List<string> v in m_DicProperty.Values)
            {
                v.Clear();
            }
            m_DicProperty.Clear();
        }
    }
    /// <summary>
    /// 分析shader 实体对象
    /// </summary>
    /// <param name="shaderObject"></param>
    public void Parse(Shader shaderObject)
    {
        if (shaderObject == null)
            return;
        // 获得变体数量
        System.Type t2 = typeof(ShaderUtil);
        MethodInfo method = t2.GetMethod("GetVariantCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var variantCount = method.Invoke(null, new System.Object[] { shaderObject, true });
        this.variantCount = int.Parse(variantCount.ToString());

        // 是否支持SRP
        MethodInfo method2 = t2.GetMethod("GetSRPBatcherCompatibilityCode", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var code = method2.Invoke(null, new System.Object[] { shaderObject, 0 });
        this.isSupportSRP = int.Parse(code.ToString()) == 0;

        int count = shaderObject.GetPropertyCount();
        for (int i = 0; i < count; i++)
        {
            ShaderPropertyType type = shaderObject.GetPropertyType(i);
            string propertyName = shaderObject.GetPropertyName(i);
            AddPropertyData(type, propertyName);
        }
    }
    /// <summary>
    /// 添加shader 属性数据
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    private void AddPropertyData(ShaderPropertyType type, string propertyName)
    {
        if (m_DicProperty.ContainsKey(type) == true)
        {
            m_DicProperty[type].Add(propertyName);
        }
        else
        {
            m_DicProperty.Add(type, new List<string> { propertyName });
        }
    }
    /// <summary>
    /// 获取纹理数量
    /// </summary>
    /// <returns></returns>
    public int GetTextureCount()
    {
        if (m_DicProperty == null || m_DicProperty.Count == 0)
            return 0;
        List<string> ret;
        m_DicProperty.TryGetValue(ShaderPropertyType.Texture, out ret);
        if (ret == null || ret.Count == 0)
            return 0;
        else return ret.Count;
    }


}
