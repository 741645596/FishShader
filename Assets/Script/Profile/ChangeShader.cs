using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// https://blog.csdn.net/liyaxin2010/article/details/83449410
/// 
/// </summary>
public class ChangeShader : MonoBehaviour
{
    public Shader myShader;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// 只替换当前帧
    /// </summary>
    public void RenderWithShader()
    {
        
    }

    /// <summary>
    /// 之后所有帧都被替换了
    /// </summary>
    public void SetReplacementShader()
    {
        // 用myshader 去替换场景中的shader 进行渲染
        // 所有的物件都使用myShader进行渲染
        //Camera.main.SetReplacementShader(myShader, "RenderType");
        //1. 首先在场景中找到标签中包含该字符串（这里为“RenderType”）的Shader
        //2. 再去看该字符串对应的数值是否与Shader1中该字符串的值一致，如果一致，则替代渲染，否则不渲染
        Camera.main.SetReplacementShader(myShader, "RenderType");
        // 比较tag，并且确定tag内容一致
        //Camera.main.SetReplacementShader(myShader, "CheckRenderTypeTag");
    }
}
